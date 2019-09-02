namespace BuildingRegistry.Projections.Syndication
{
    using Address;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Features.OwnedInstances;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Parcel;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class Program
    {
        private static readonly AutoResetEvent Closing = new AutoResetEvent(false);
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            var ct = CancellationTokenSource.Token;

            ct.Register(() => Closing.Set());
            Console.CancelKeyPress += (sender, eventArgs) => CancellationTokenSource.Cancel();

            Console.WriteLine("Starting BuildingRegistry.Projections.Syndication");

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
                .AddCommandLine(args ?? new string[0])
                .Build();

            var container = ConfigureServices(configuration);

            try
            {
                await MigrationsHelper.RunAsync(
                    configuration.GetConnectionString("SyndicationProjectionsAdmin"),
                    container.GetService<ILoggerFactory>(),
                    ct);

                await Task.WhenAll(StartRunners(configuration, container, ct));

                Console.WriteLine("Running... Press CTRL + C to exit.");
                Closing.WaitOne();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                Thread.Sleep(1000);
                throw;
            }

            Console.WriteLine("Stopping...");
            Closing.Close();
        }

        private static IEnumerable<Task> StartRunners(IConfiguration configuration, IServiceProvider container, CancellationToken ct)
        {
            var addressRunner = new FeedProjectionRunner<AddressEvent, SyndicationContent<Address.Address>, SyndicationContext>(
                "address",
                configuration.GetValue<Uri>("SyndicationFeeds:Address"),
                configuration.GetValue<string>("SyndicationFeeds:AddressAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:AddressAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:AddressPollingInMilliseconds"),
                false,
                true,
                container.GetService<ILogger<Program>>(),
                container.GetService<IRegistryAtomFeedReader>(),
                new AddressPersistentLocalIdProjection());

            yield return addressRunner.CatchUpAsync(
                  container.GetService<Func<Owned<SyndicationContext>>>(),
                  ct);

            var parcelRunner = new FeedProjectionRunner<ParcelEvent, SyndicationContent<Parcel.Parcel>, SyndicationContext>(
                "parcel",
                configuration.GetValue<Uri>("SyndicationFeeds:Parcel"),
                configuration.GetValue<string>("SyndicationFeeds:ParcelAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:ParcelAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:ParcelPollingInMilliseconds"),
                false,
                true,
                container.GetService<ILogger<Program>>(),
                container.GetService<IRegistryAtomFeedReader>(),
                new BuildingParcelLatestProjection());

            yield return parcelRunner.CatchUpAsync(
                container.GetService<Func<Owned<SyndicationContext>>>(),
                ct);
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            builder.RegisterModule(new SyndicationModule(configuration, services, tempProvider.GetService<ILoggerFactory>()));

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
