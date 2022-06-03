namespace BuildingRegistry.Migrator.Building.Infrastructure.Modules
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using BuildingRegistry.Infrastructure;
    using BuildingRegistry.Infrastructure.Modules;
    using Consumer.Address;
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
            // TODO: backoffice
            //var projectionsConnectionString = _configuration.GetConnectionString("BackOffice");
            //_services
            //    .AddDbContext<BackOfficeContext>(options => options
            //            .UseLoggerFactory(_loggerFactory)
            //            .UseSqlServer(projectionsConnectionString, sqlServerOptions => sqlServerOptions
            //                .EnableRetryOnFailure()
            //                .MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice))
            //        , ServiceLifetime.Transient);

            builder
                .RegisterModule(new DataDogModule(_configuration))
                .RegisterModule<EnvelopeModule>()
                .RegisterModule(new EditModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new ConsumerModule(_configuration, _services, _loggerFactory));

            builder.RegisterEventstreamModule(_configuration);

            builder.Populate(_services);
        }
    }
}
