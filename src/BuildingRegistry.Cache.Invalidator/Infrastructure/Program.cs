namespace BuildingRegistry.Cache.Invalidator.Infrastructure
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Consumer.Read.Parcel;
    using Consumer.Read.Parcel.Infrastructure.Modules;
    using Destructurama;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Polly;
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

            Log.Information("Starting BuildingRegistry.Cache.Invalidator");

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
                .ConfigureServices((hostContext, services) => { })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);
                    var services = new ServiceCollection();

                    builder.RegisterModule(new ConsumerParcelModule(hostContext.Configuration, services, loggerFactory));

                    builder
                        .RegisterType<RedisCacheInvalidateService>()
                        .As<IRedisCacheInvalidateService>()
                        .SingleInstance();

                    builder
                        .RegisterType<CacheInvalidator>()
                        .As<IHostedService>()
                        .SingleInstance();

                    builder.Populate(services);
                })
                .UseConsoleLifetime()
                .Build();

            Log.Information("Starting BuildingRegistry.Cache.Invalidator");

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var consumerParcelContext = host.Services.GetRequiredService<ConsumerParcelContext>();

            try
            {
                if (consumerParcelContext.BuildingsToInvalidate.Any())
                {
                    var distributedLockTableName = configuration.GetValue<string>("DistributedLock:LockName")
                                                   ?? throw new Exception("No 'LockName' configuration found");

                    var distributedLockOptions = DistributedLockOptions.LoadFromConfiguration(configuration);
                    var distributedLock = new DistributedLock<Program>(
                        distributedLockOptions,
                        distributedLockTableName,
                        loggerFactory.CreateLogger<Program>());

                    await Policy
                        .Handle<AcquireLockFailedException>()
                        .WaitAndRetryAsync(5, _ =>
                        {
                            Log.Information("Failed to acquire lock. Trying again within 1 minute.");
                            return TimeSpan.FromMinutes(1);
                        })
                        .ExecuteAsync(async ct =>
                        {
                            await distributedLock.RunAsync(
                                    async () => { await host.RunAsync(token: ct).ConfigureAwait(false); })
                                .ConfigureAwait(false);
                        }, CancellationToken.None);
                }
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
