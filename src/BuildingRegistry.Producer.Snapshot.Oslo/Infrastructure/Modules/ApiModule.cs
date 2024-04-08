namespace BuildingRegistry.Producer.Snapshot.Oslo.Infrastructure.Modules
{
    using System;
    using System.Net.Http;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using BuildingRegistry.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ApiModule(
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

            RegisterProjections(builder);
        }

        private void RegisterProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new ProducerModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var connectedProjectionSettings = ConnectedProjectionSettings.Configure(x =>
            {
                x.ConfigureCatchUpPageSize(ConnectedProjectionSettings.Default.CatchUpPageSize);
                x.ConfigureCatchUpUpdatePositionMessageInterval(Convert.ToInt32(_configuration["CatchUpSaveInterval"]));
            });

            var bootstrapServers = _configuration["Kafka:BootstrapServers"];
            var saslUsername = _configuration["Kafka:SaslUserName"];
            var saslPassword = _configuration["Kafka:SaslPassword"];

            var maxRetryWaitIntervalSeconds = _configuration["RetryPolicy:MaxRetryWaitIntervalSeconds"];
            var retryBackoffFactor = _configuration["RetryPolicy:RetryBackoffFactor"];

            builder
                .RegisterProjectionMigrator<ProducerContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<ProducerBuildingProjections, ProducerContext>(c =>
                {
                    var osloNamespace = _configuration["BuildingOsloNamespace"].TrimEnd('/');

                        var topic = $"{_configuration[ProducerBuildingProjections.TopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {ProducerBuildingProjections.TopicKey}");
                        var producerOptions = new ProducerOptions(
                                new BootstrapServers(bootstrapServers),
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

                        return new ProducerBuildingProjections(
                            new Producer(producerOptions),
                            new SnapshotManager(
                                c.Resolve<ILoggerFactory>(),
                                new OsloProxy(new HttpClient
                                {
                                    BaseAddress = new Uri(_configuration["BuildingOsloApiUrl"].TrimEnd('/')),
                                }),
                                SnapshotManagerOptions.Create(
                                    maxRetryWaitIntervalSeconds,
                                    retryBackoffFactor)),
                            osloNamespace);
                    },
                    connectedProjectionSettings)
                .RegisterProjections<ProducerBuildingUnitProjections, ProducerContext>(c =>
                {
                    var osloNamespace = _configuration["BuildingUnitOsloNamespace"].TrimEnd('/');

                    var topic = $"{_configuration[ProducerBuildingUnitProjections.TopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {ProducerBuildingProjections.TopicKey}");
                    var producerOptions = new ProducerOptions(
                            new BootstrapServers(bootstrapServers),
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

                    return new ProducerBuildingUnitProjections(
                        new Producer(producerOptions),
                        new SnapshotManager(
                            c.Resolve<ILoggerFactory>(),
                            new OsloProxy(new HttpClient
                            {
                                BaseAddress = new Uri(_configuration["BuildingUnitOsloApiUrl"].TrimEnd('/')),
                            }),
                            SnapshotManagerOptions.Create(
                                maxRetryWaitIntervalSeconds,
                                retryBackoffFactor)),
                        osloNamespace);
                },
                    connectedProjectionSettings);
        }
    }
}
