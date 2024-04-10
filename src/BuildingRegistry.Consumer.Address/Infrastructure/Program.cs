namespace BuildingRegistry.Consumer.Address.Infrastructure
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Building;
    using BuildingRegistry.Infrastructure;
    using BuildingRegistry.Infrastructure.Modules;
    using Destructurama;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Extensions.Logging;

    public sealed class Program
    {
        protected Program()
        { }

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            Log.Information("Starting BuildingRegistry.Consumer.Address");

            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((hostContext, builder) =>
                {
                    SelfLog.Enable(Console.WriteLine);

                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        .Enrich.WithEnvironmentUserName()
                        .Destructure.JsonNetTypes()
                        .CreateLogger();

                    builder.ClearProviders();
                    builder.AddSerilog(Log.Logger);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);

                    services
                        .AddDbContextFactory<BackOfficeContext>((provider, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(hostContext.Configuration.GetConnectionString("BackOffice"), sqlServerOptions => sqlServerOptions
                                .EnableRetryOnFailure()
                                .MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice)
                            ));

                    services
                        .AddDbContextFactory<ConsumerAddressContext>((provider, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(hostContext.Configuration.GetConnectionString("ConsumerAddress"), sqlServerOptions =>
                            {
                                sqlServerOptions.EnableRetryOnFailure();
                                sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerAddress, Schema.ConsumerAddress);
                            }));

                    services.AddScoped<IAddresses, ConsumerAddressContext>();
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    var services = new ServiceCollection();
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);

                    builder.Register(c =>
                    {
                        var bootstrapServers = hostContext.Configuration["Kafka:BootstrapServers"];
                        var topic = $"{hostContext.Configuration["AddressTopic"]}" ?? throw new ArgumentException("Configuration has no AddressTopic.");
                        var suffix = hostContext.Configuration["GroupSuffix"];
                        var consumerGroupId = $"BuildingRegistry.ConsumerAddress.{topic}{suffix}";

                        var consumerOptions = new ConsumerOptions(
                            new BootstrapServers(bootstrapServers),
                            new Topic(topic),
                            new ConsumerGroupId(consumerGroupId),
                            EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                        consumerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                            hostContext.Configuration["Kafka:SaslUserName"],
                            hostContext.Configuration["Kafka:SaslPassword"]));

                        var offsetStr = hostContext.Configuration["AddressTopicOffset"];
                        if (!string.IsNullOrEmpty(offsetStr) && long.TryParse(offsetStr, out var offset))
                        {
                            var ignoreDataCheck = hostContext.Configuration.GetValue<bool>("IgnoreAddressTopicOffsetDataCheck", false);

                            if (!ignoreDataCheck)
                            {
                                using var ctx = c.Resolve<ConsumerAddressContext>();

                                if (ctx.AddressConsumerItems.Any())
                                {
                                    throw new InvalidOperationException(
                                        $"Cannot set Kafka offset to {offset} because {nameof(ctx.AddressConsumerItems)} has data.");
                                }
                            }

                            consumerOptions.ConfigureOffset(new Offset(offset));
                        }

                        return consumerOptions;
                    });

                    builder
                        .RegisterType<IdempotentConsumer<ConsumerAddressContext>>()
                        .As<IIdempotentConsumer<ConsumerAddressContext>>()
                        .SingleInstance();

                    builder
                        .RegisterModule(new CommandHandlingModule(hostContext.Configuration))
                        .RegisterModule(new BackOfficeModule(hostContext.Configuration, services, loggerFactory))
                        .RegisterModule(new SequenceModule(hostContext.Configuration, services, loggerFactory));

                    builder
                        .RegisterType<ConsumerAddress>()
                        .As<IHostedService>()
                        .SingleInstance();

                    builder.Populate(services);
                })
                .UseConsoleLifetime()
                .Build();

            Log.Information("Starting BuildingRegistry.Consumer.Address");

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            try
            {
                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
                        await MigrationsHelper.RunAsync(
                            configuration.GetConnectionString("ConsumerAddressAdmin"),
                            loggerFactory,
                            CancellationToken.None);

                        await host.RunAsync().ConfigureAwait(false);
                    },
                    DistributedLockOptions.LoadFromConfiguration(configuration),
                    logger)
                    .ConfigureAwait(false);
            }
            catch (AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    logger.LogCritical(innerException, "Encountered a fatal exception, exiting program.");
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                await Task.Delay(500, default);
                throw;
            }
            finally
            {
                logger.LogInformation("Stopping...");
            }
        }
    }
}
