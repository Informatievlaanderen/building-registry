namespace BuildingRegistry.Grb.Abstractions
{
    using Autofac;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class BuildingGrbModule : Module
    {
        public BuildingGrbModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var projectionsConnectionString = configuration.GetConnectionString("BuildingGrb");

            services
                .AddDbContext<BuildingGrbContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(projectionsConnectionString,
                        sqlServerOptions => sqlServerOptions
                            .MigrationsHistoryTable(BuildingGrbContext.MigrationsTableName, BuildingGrbContext.Schema)
                            .UseNetTopologySuite()
                    ));
        }
    }
}
