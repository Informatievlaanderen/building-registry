namespace BuildingRegistry.Producer.Snapshot.Oslo.Infrastructure.Modules
{
    using System;
    using System.Net.Http;
    using Amazon.SimpleNotificationService;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Notifications;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.SqlServer.MigrationExtensions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NodaTime;

    public class ProducerModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ProducerModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(_ => SystemClock.Instance)
                .As<IClock>()
                .SingleInstance();

            RegisterProjectionSetup(builder);

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Populate(_services);
        }

        private void RegisterProjectionSetup(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new EventHandlingModule(
                        typeof(DomainAssemblyMarker).Assembly,
                        EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))
                .RegisterModule<EnvelopeModule>()
                .RegisterEventstreamModule(_configuration)
                .RegisterModule(new ProjectorModule(_configuration));

            var logger = _loggerFactory.CreateLogger<ProducerModule>();
            var connectionString = _configuration.GetConnectionString("ProducerSnapshotProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(_services, _loggerFactory, connectionString);
            }
            else
            {
                RunInMemoryDb(_services, _loggerFactory, logger);
            }

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(ProducerContext), Schema.ProducerSnapshotOslo, MigrationTables.ProducerSnapshotOslo);

            RegisterProjections(builder);
            RegisterReproducers();
        }

        private void RegisterProjections(ContainerBuilder builder)
        {
            var connectedProjectionSettings = ConnectedProjectionSettings.Configure(x =>
            {
                x.ConfigureCatchUpPageSize(ConnectedProjectionSettings.Default.CatchUpPageSize);
                x.ConfigureCatchUpUpdatePositionMessageInterval(Convert.ToInt32(_configuration["CatchUpSaveInterval"]));
            });

            var maxRetryWaitIntervalSeconds = _configuration["RetryPolicy:MaxRetryWaitIntervalSeconds"]!;
            var retryBackoffFactor = _configuration["RetryPolicy:RetryBackoffFactor"]!;

            builder
                .RegisterProjectionMigrator<ProducerContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<ProducerBuildingProjections, ProducerContext>(c =>
                    {
                        var osloNamespace = _configuration["BuildingOsloNamespace"]!.TrimEnd('/');
                        var producerOptions = CreateBuildingProducerOptions();

                        var osloProxy = new OsloProxy(new HttpClient
                        {
                            BaseAddress = new Uri(_configuration["BuildingOsloApiUrl"]!.TrimEnd('/')),
                        });

                        return new ProducerBuildingProjections(
                            new Producer(producerOptions),
                            new SnapshotManager(
                                c.Resolve<ILoggerFactory>(),
                                osloProxy,
                                SnapshotManagerOptions.Create(
                                    maxRetryWaitIntervalSeconds,
                                    retryBackoffFactor)),
                            osloNamespace,
                            osloProxy);
                    },
                    connectedProjectionSettings)
                .RegisterProjections<ProducerBuildingUnitProjections, ProducerContext>(c =>
                    {
                        var osloNamespace = _configuration["BuildingUnitOsloNamespace"]!.TrimEnd('/');
                        var producerOptions = CreateBuildingUnitProducerOptions();

                        var osloProxy = new OsloProxy(new HttpClient
                        {
                            BaseAddress = new Uri(_configuration["BuildingUnitOsloApiUrl"]!.TrimEnd('/')),
                        });

                        return new ProducerBuildingUnitProjections(
                            new Producer(producerOptions),
                            new SnapshotManager(
                                c.Resolve<ILoggerFactory>(),
                                osloProxy,
                                SnapshotManagerOptions.Create(
                                    maxRetryWaitIntervalSeconds,
                                    retryBackoffFactor)),
                            osloNamespace,
                            osloProxy);
                    },
                    connectedProjectionSettings);
        }

        private void RegisterReproducers()
        {
            _services.AddAWSService<IAmazonSimpleNotificationService>();
            _services.AddSingleton<INotificationService>(sp =>
                new NotificationService(sp.GetRequiredService<IAmazonSimpleNotificationService>(),
                    _configuration.GetValue<string>("NotificationTopicArn")!));

            var connectionString = _configuration.GetConnectionString("Integration");
            var utcHourToRunWithin = _configuration.GetValue<int>("SnapshotReproducerUtcHour");

            _services.AddHostedService<BuildingSnapshotReproducer>(provider =>
            {
                var producerOptions = CreateBuildingProducerOptions();

                return new BuildingSnapshotReproducer(
                    connectionString!,
                    new OsloProxy(new HttpClient
                    {
                        BaseAddress = new Uri(_configuration["BuildingOsloApiUrl"]!.TrimEnd('/')),
                    }),
                    new Producer(producerOptions),
                    provider.GetRequiredService<IClock>(),
                    provider.GetRequiredService<INotificationService>(),
                    utcHourToRunWithin,
                    _loggerFactory);
            });

            _services.AddHostedService<BuildingUnitSnapshotReproducer>(provider =>
            {
                var producerOptions = CreateBuildingUnitProducerOptions();

                return new BuildingUnitSnapshotReproducer(
                    connectionString!,
                    new OsloProxy(new HttpClient
                    {
                        BaseAddress = new Uri(_configuration["BuildingUnitOsloApiUrl"]!.TrimEnd('/')),
                    }),
                    new Producer(producerOptions),
                    provider.GetRequiredService<IClock>(),
                    provider.GetRequiredService<INotificationService>(),
                    utcHourToRunWithin,
                    _loggerFactory);
            });
        }

        private ProducerOptions CreateBuildingProducerOptions()
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"];
            var saslUsername = _configuration["Kafka:SaslUserName"];
            var saslPassword = _configuration["Kafka:SaslPassword"];

            var topic = $"{_configuration[ProducerBuildingProjections.TopicKey]}" ??
                        throw new ArgumentException($"Configuration has no value for {ProducerBuildingProjections.TopicKey}");
            var producerOptions = new ProducerOptions(
                    new BootstrapServers(bootstrapServers!),
                    new Topic(topic),
                    true,
                    EventsJsonSerializerSettingsProvider.CreateSerializerSettings())
                .ConfigureEnableIdempotence();

            if (!string.IsNullOrEmpty(saslUsername)
                && !string.IsNullOrEmpty(saslPassword))
            {
                producerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                    saslUsername,
                    saslPassword));
            }

            return producerOptions;
        }

        private ProducerOptions CreateBuildingUnitProducerOptions()
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"];
            var saslUsername = _configuration["Kafka:SaslUserName"];
            var saslPassword = _configuration["Kafka:SaslPassword"];

            var topic = $"{_configuration[ProducerBuildingUnitProjections.TopicKey]}" ??
                        throw new ArgumentException($"Configuration has no value for {ProducerBuildingProjections.TopicKey}");
            var producerOptions = new ProducerOptions(
                    new BootstrapServers(bootstrapServers!),
                    new Topic(topic),
                    true,
                    EventsJsonSerializerSettingsProvider.CreateSerializerSettings())
                .ConfigureEnableIdempotence();
            if (!string.IsNullOrEmpty(saslUsername)
                && !string.IsNullOrEmpty(saslPassword))
            {
                producerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                    saslUsername,
                    saslPassword));
            }

            return producerOptions;
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string producerSnapshotConnectionString)
        {
            services
                .AddDbContext<ProducerContext>((_, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(producerSnapshotConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ProducerSnapshotOslo, Schema.ProducerSnapshotOslo);
                    })
                    .UseExtendedSqlServerMigrations());
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<ProducerContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ProducerContext));
        }
    }
}
