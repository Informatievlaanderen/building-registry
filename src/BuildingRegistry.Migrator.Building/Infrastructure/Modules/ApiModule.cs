namespace BuildingRegistry.Migrator.Building.Infrastructure.Modules
{
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using BuildingRegistry.Infrastructure;
    using BuildingRegistry.Infrastructure.Modules;
    using BuildingRegistry.Migrator.Building.Projections;
    using Consumer.Address;
    using Microsoft.EntityFrameworkCore;
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
            var backOfficeConnectionString = _configuration.GetConnectionString("BackOffice");
            _services
                .AddDbContextFactory<BackOfficeContext>(options => options
                        .UseLoggerFactory(_loggerFactory)
                        .UseSqlServer(backOfficeConnectionString, sqlServerOptions => sqlServerOptions
                            .EnableRetryOnFailure()
                            .MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice))
                    , ServiceLifetime.Transient);

            builder
                .RegisterModule(new DataDogModule(_configuration))
                .RegisterModule<EnvelopeModule>()
                .RegisterModule(new EditModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new ConsumerAddressModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new MigratorConsumerModule(_configuration, _services, _loggerFactory));

            builder.RegisterModule(new ProjectorModule(_configuration));

            builder
                .RegisterProjectionMigrator<MigratorConsumerContextFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<MigratorConsumerProjection, MigratorConsumerContext>(
                    context => new MigratorConsumerProjection(context.Resolve<ILoggerFactory>().CreateLogger<MigratorConsumerProjection>(), context.Resolve<IDbContextFactory<BackOfficeContext>>()),
                    ConnectedProjectionSettings.Configure(a =>
                    {
                        a.ConfigureCatchUpUpdatePositionMessageInterval(1);
                    }));

            builder.RegisterEventstreamModule(_configuration);
            builder.RegisterSnapshotModule(_configuration);

            builder.Populate(_services);
        }
    }
}
