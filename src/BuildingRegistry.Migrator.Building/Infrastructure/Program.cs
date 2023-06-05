namespace BuildingRegistry.Migrator.Building.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using BuildingRegistry.Migrator.Building.Projections;
    using Consumer.Address;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Polly;
    using Serilog;

    public class Program
    {
        protected Program()
        { }

        public static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var ct = cancellationTokenSource.Token;

            var closing = new AutoResetEvent(false);
            ct.Register(() => closing.Set());
            Console.CancelKeyPress += (sender, eventArgs) => cancellationTokenSource.Cancel();

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var container = ConfigureServices(configuration);

            Log.Information("Starting BuildingRegistry.Migrator");

            try
            {
                await MigrationsHelper.RunAsync(
                    configuration.GetConnectionString("events"),
                    container.GetRequiredService<ILoggerFactory>(),
                    ct);

                var watch = Stopwatch.StartNew();

                Dictionary<Guid, int> consumedAddressItems;
                await using (var consumerContext = container.GetRequiredService<ConsumerAddressContext>())
                {
                    consumedAddressItems = await consumerContext
                        .AddressConsumerItems
                        .Where(x => x.AddressId.HasValue)
                        .Select(x => new { AddressId = x.AddressId!.Value, x.AddressPersistentLocalId })
                        .ToDictionaryAsync(x => x.AddressId, y => y.AddressPersistentLocalId, ct);
                }

                var migrator = new StreamMigrator(
                    container.GetRequiredService<ILoggerFactory>(),
                    configuration,
                    container.GetRequiredService<ILifetimeScope>(),
                    consumedAddressItems);

                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
                        try
                        {
                            await Policy
                                .Handle<SqlException>()
                                .WaitAndRetryAsync(60, _ => TimeSpan.FromSeconds(60),
                                    (_, timespan) =>
                                        Log.Information($"SqlException occurred retrying after {timespan.Seconds} seconds."))
                                .ExecuteAsync(async () =>
                                {
                                    await migrator.ProcessAsync(ct);

                                    var sqlStreamTable = new SqlStreamsTable(configuration.GetConnectionString("events"));
                                    var startingMigrationPosition = await sqlStreamTable.GetStartingMigrationPosition();

                                    var projectorRunner = new ProjectorRunner(
                                        container.GetRequiredService<IConnectedProjectionsManager>(),
                                        container.GetRequiredService<ILoggerFactory>(),
                                        configuration.GetConnectionString("events")
                                        );
                                    Log.Information("The projection consumer was started");
                                    await projectorRunner.StartAsync(startingMigrationPosition, ct);

                                });

                            watch.Stop();
                            Log.Information($"Migration finished, time elapsed '{watch.Elapsed:g}'");
                        }
                        catch (Exception e)
                        {
                            Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                            throw;
                        }
                    },
                    DistributedLockOptions.LoadFromConfiguration(configuration),
                    container.GetRequiredService<ILogger<Program>>());
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                await Task.Delay(1000, default);
                throw;
            }

            Log.Information("Stopping...");
            closing.Close();
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            var loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();

            builder.RegisterModule(new ApiModule(configuration, services, loggerFactory));
            builder.RegisterModule(new ProjectorModule(configuration));

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
