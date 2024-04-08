namespace BuildingRegistry.Migrator.Building.Projections
{
    using System;
    using Autofac;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class MigratorProjectionModule : Module
    {
        public MigratorProjectionModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var logger = loggerFactory.CreateLogger<MigratorProjectionContext>();
            var connectionString = configuration.GetConnectionString("Events");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(services, serviceLifetime, loggerFactory, connectionString);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ServiceLifetime serviceLifetime,
            ILoggerFactory loggerFactory,
            string eventsConnectionString)
        {
            services
                .AddDbContext<MigratorProjectionContext>((_, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(eventsConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.MigratorProjection, Schema.MigrateBuilding);
                    }), serviceLifetime);
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger logger)
        {
            services
                .AddDbContext<MigratorProjectionContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(MigratorProjectionContext));
        }
    }
}
