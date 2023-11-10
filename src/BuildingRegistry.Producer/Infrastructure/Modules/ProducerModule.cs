namespace BuildingRegistry.Producer.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using BuildingRegistry.Infrastructure;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

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
            _services.RegisterModule(new DataDogModule(_configuration));

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
            var logger = _loggerFactory.CreateLogger<ProducerModule>();
            var connectionString = _configuration.GetConnectionString("ProducerProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(_configuration, _services, _loggerFactory, connectionString);
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
                nameof(ProducerContext), Schema.Producer, MigrationTables.Producer);

            var connectedProjectionSettings = ConnectedProjectionSettings.Configure(x =>
            {
                x.ConfigureCatchUpPageSize(ConnectedProjectionSettings.Default.CatchUpPageSize);
                x.ConfigureCatchUpUpdatePositionMessageInterval(Convert.ToInt32(_configuration["CatchUpSaveInterval"]));
            });

            var saslUserName = _configuration["Kafka:SaslUserName"];
            var saslPassword = _configuration["Kafka:SaslPassword"];
            var bootstrapServers = _configuration["Kafka:BootstrapServers"];

            builder
                .RegisterProjectionMigrator<ProducerContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                // TODO: see if we still need to implements this
                // .RegisterProjections<ProducerProjections, ProducerContext>(() =>
                // {
                //     var topic = $"{_configuration[ProducerProjections.TopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {ProducerProjections.TopicKey}");
                //     var producerOptions = new ProducerOptions(
                //         new BootstrapServers(bootstrapServers),
                //         new Topic(topic),
                //         true,
                //         EventsJsonSerializerSettingsProvider.CreateSerializerSettings())
                //         .ConfigureEnableIdempotence();
                //     if (!string.IsNullOrEmpty(saslUserName)
                //         && !string.IsNullOrEmpty(saslPassword))
                //     {
                //         producerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                //             saslUserName,
                //             saslPassword));
                //     }
                //
                //     return new ProducerProjections(new Producer(producerOptions));
                // }, connectedProjectionSettings)
                .RegisterProjections<ProducerMigrateProjections, ProducerContext>(() =>
                {
                    var topic = $"{_configuration[ProducerMigrateProjections.TopicKey]}" ?? throw new ArgumentException($"Configuration has no value for {ProducerMigrateProjections.TopicKey}");
                    var producerOptions = new ProducerOptions(
                            new BootstrapServers(bootstrapServers),
                            new Topic(topic),
                            true,
                            EventsJsonSerializerSettingsProvider.CreateSerializerSettings())
                        .ConfigureEnableIdempotence();

                    if (!string.IsNullOrEmpty(saslUserName)
                        && !string.IsNullOrEmpty(saslPassword))
                    {
                        producerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                            saslUserName,
                            saslPassword));
                    }

                    return new ProducerMigrateProjections(new Producer(producerOptions));
                }, connectedProjectionSettings);
        }

        private static void RunOnSqlServer(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string backofficeProjectionsConnectionString)
        {
            services
                .AddScoped(_ => new TraceDbConnection<ProducerContext>(
                    new SqlConnection(backofficeProjectionsConnectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<ProducerContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<ProducerContext>>(), sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Producer, Schema.Producer);
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
