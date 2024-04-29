namespace BuildingRegistry.Api.BackOffice.Abstractions
{
    using Autofac;
    using BuildingRegistry.Building;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class BackOfficeModule : Module
    {
        public BackOfficeModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var projectionsConnectionString = configuration.GetConnectionString("BackOffice");

            services
                .AddDbContext<BackOfficeContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(projectionsConnectionString, sqlServerOptions => sqlServerOptions
                            // .EnableRetryOnFailure() Commented to enable transactions
                            .MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice)
                    ));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<AddCommonBuildingUnit>()
                .As<IAddCommonBuildingUnit>();

            base.Load(builder);
        }
    }
}
