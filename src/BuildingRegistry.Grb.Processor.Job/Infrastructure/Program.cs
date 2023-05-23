namespace BuildingRegistry.Grb.Processor.Job.Infrastructure
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Abstractions;
    using Amazon.S3;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Destructurama;
    using Microsoft.Data.SqlClient;
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
        private Program()
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

            Log.Information("Starting BuildingRegistry.Grb.Processor.Job.");

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
                        .AddScoped(s => new TraceDbConnection<BuildingGrbContext>(
                            new SqlConnection(hostContext.Configuration.GetConnectionString("BuildingGrb")),
                            hostContext.Configuration["DataDog:ServiceName"]))
                        .AddDbContextFactory<BuildingGrbContext>((provider, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(provider.GetRequiredService<TraceDbConnection<BuildingGrbContext>>(), sqlServerOptions => sqlServerOptions
                                .EnableRetryOnFailure()
                                .MigrationsHistoryTable(BuildingGrbContext.MigrationsTableName, BuildingGrbContext.Schema)
                            ))
                        .AddHttpClient(nameof(BackOfficeApiProxy), client =>
                        {
                            client.BaseAddress = new Uri(hostContext.Configuration["BackOfficeApiUrl"]);
                        });
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    var services = new ServiceCollection();
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);

                    builder
                        .RegisterModule(new DataDogModule(hostContext.Configuration))
                        .RegisterModule(new BuildingGrbModule(hostContext.Configuration, services, loggerFactory))
                        .RegisterModule(new TicketingModule(hostContext.Configuration, services));

                    builder.RegisterType<BackOfficeApiProxy>()
                        .As<IBackOfficeApiProxy>()
                        .SingleInstance();

                    builder.RegisterType<JobRecordsProcessor>()
                        .As<IJobRecordsProcessor>()
                        .SingleInstance();

                    builder.RegisterType<JobRecordsMonitor>()
                        .As<IJobRecordsMonitor>()
                        .SingleInstance();

                    builder.RegisterType<JobResultUploader>()
                        .As<IJobResultUploader>()
                        .SingleInstance();

                    builder
                        .Register(_ =>
                            new S3BlobClient(new AmazonS3Client(),hostContext.Configuration["BucketName"]))
                        .As<IBlobClient>()
                        .SingleInstance();

                    builder
                        .Register(_ => new JobRecordsArchiver(hostContext.Configuration.GetConnectionString("BuildingGrb"), loggerFactory))
                        .As<IJobRecordsArchiver>()
                        .SingleInstance();

                    builder
                        .RegisterType<JobProcessor>()
                        .As<IHostedService>()
                        .SingleInstance();

                    builder.Populate(services);
                })
                .UseConsoleLifetime()
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            try
            {
                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
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
