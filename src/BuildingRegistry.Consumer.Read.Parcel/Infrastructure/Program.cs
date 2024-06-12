namespace BuildingRegistry.Consumer.Read.Parcel.Infrastructure
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using BuildingRegistry.Infrastructure;
    using Destructurama;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using ParcelWithCount;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Extensions.Logging;

    public sealed class Program
    {
        protected Program()
        { }

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += (_, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            Log.Information("Starting BuildingRegistry.Consumer.Read.Parcel");

            var host = new HostBuilder()
                .ConfigureAppConfiguration((_, builder) =>
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
                        .AddDbContextFactory<ConsumerParcelContext>((_, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(hostContext.Configuration.GetConnectionString("ConsumerParcel"), sqlServerOptions =>
                            {
                                sqlServerOptions.EnableRetryOnFailure();
                                sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadParcel, Schema.ConsumerReadParcel);
                                sqlServerOptions.UseNetTopologySuite();
                            }));
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    var services = new ServiceCollection();

                    builder
                        .Register(c =>
                        {
                            var bootstrapServers = hostContext.Configuration["Kafka:BootstrapServers"]!;
                            var topic = $"{hostContext.Configuration["Topic"]}" ?? throw new ArgumentException("Configuration has no Topic.");
                            var suffix = hostContext.Configuration["GroupSuffix"];
                            var consumerGroupId = $"BuildingRegistry.ConsumerParcelItemWithCount.{topic}{suffix}";

                            var consumerOptions = new ConsumerOptions(
                                new BootstrapServers(bootstrapServers),
                                new Topic(topic),
                                new ConsumerGroupId(consumerGroupId),
                                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                            consumerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                                hostContext.Configuration["Kafka:SaslUserName"]!,
                                hostContext.Configuration["Kafka:SaslPassword"]!));

                            var offsetStr = hostContext.Configuration["TopicOffset"];
                            if (!string.IsNullOrEmpty(offsetStr) && long.TryParse(offsetStr, out var offset))
                            {
                                var ignoreDataCheck = hostContext.Configuration.GetValue("IgnoreTopicOffsetDataCheck", false);
                                if (!ignoreDataCheck)
                                {
                                    using var ctx = c.Resolve<ConsumerParcelContext>();

                                    if (ctx.ParcelConsumerItemsWithCount.Any())
                                    {
                                        throw new InvalidOperationException(
                                            $"Cannot set Kafka offset to {offset} because {nameof(ctx.ParcelConsumerItemsWithCount)} has data.");
                                    }
                                }

                                consumerOptions.ConfigureOffset(new Offset(offset));
                            }

                            return new Consumer(consumerOptions, c.Resolve<ILoggerFactory>());
                        })
                        .As<IConsumer>()
                        .SingleInstance();

                    builder
                        .RegisterType<ConsumerParcel>()
                        .As<IHostedService>()
                        .SingleInstance();

                    builder.Populate(services);
                })
                .UseConsoleLifetime()
                .Build();

            Log.Information("Starting BuildingRegistry.Consumer.Read.Parcel");

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            try
            {
                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
                        await MigrationsHelper.RunAsync(
                            configuration.GetConnectionString("ConsumerParcelAdmin")!,
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
