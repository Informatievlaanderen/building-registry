namespace BuildingRegistry.Producer.Ldes.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
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
    using Newtonsoft.Json;
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
            var connectionString = _configuration.GetConnectionString("ProducerLdesProjections");

            if (!string.IsNullOrWhiteSpace(connectionString))
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
                nameof(ProducerContext), Schema.ProducerLdes, MigrationTables.ProducerLdes);

            RegisterProjections(builder);
        }

        private void RegisterProjections(ContainerBuilder builder)
        {
            var connectedProjectionSettings = ConnectedProjectionSettings.Configure(x =>
            {
                x.ConfigureCatchUpPageSize(ConnectedProjectionSettings.Default.CatchUpPageSize);
                x.ConfigureCatchUpUpdatePositionMessageInterval(Convert.ToInt32(_configuration["CatchUpSaveInterval"]));
            });

            builder
                .RegisterProjectionMigrator<ProducerContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<ProducerProjections, ProducerContext>(_ =>
                    {
                        var buildingOsloNamespace = _configuration["BuildingOsloNamespace"]!.TrimEnd('/');
                        var buildingUnitOsloNamespace = _configuration["BuildingUnitOsloNamespace"]!.TrimEnd('/');
                        var buildingProducerOptions = CreateProducerOptions("BuildingTopic");
                        var buildingUnitProducerOptions = CreateProducerOptions("BuildingUnitTopic");

                        return new ProducerProjections(
                            buildingProducer: new Producer(buildingProducerOptions),
                            buildingUnitProducer: new Producer(buildingUnitProducerOptions),
                            buildingOsloNamespace: buildingOsloNamespace,
                            buildingUnitOsloNamespace: buildingUnitOsloNamespace,
                            new JsonSerializerSettings().ConfigureDefaultForApi());
                    },
                    connectedProjectionSettings);
        }

        private ProducerOptions CreateProducerOptions(string topicConfigurationKey)
        {
            var bootstrapServers = _configuration["Kafka:BootstrapServers"];
            var saslUsername = _configuration["Kafka:SaslUserName"];
            var saslPassword = _configuration["Kafka:SaslPassword"];

            var topic = _configuration[topicConfigurationKey]
                        ?? throw new ArgumentException($"Configuration has no value for {topicConfigurationKey}");
            var producerOptions = new ProducerOptions(
                    new BootstrapServers(bootstrapServers!),
                    new Topic(topic),
                    useSinglePartition: false,
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
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.ProducerLdes, Schema.ProducerLdes);
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
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), _ => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(ProducerContext));
        }
    }
}
