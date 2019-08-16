namespace BuildingRegistry.Projections.Legacy
{
    using System;
    using Autofac;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class LegacyModule : Module
    {
        public LegacyModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<LegacyModule>();
            var legacyProjectionsConnectionString = configuration.GetConnectionString("LegacyProjections");

            services
                .AddDbContext<LegacyContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(legacyProjectionsConnectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(MigrationTables.Legacy, Schema.Legacy);
                        //sqlServerOptions.UseNetTopologySuite();
                    }));

            logger.LogInformation(
                "Added {Context} to services:" +
                Environment.NewLine +
                "\tSchema: {Schema}" +
                Environment.NewLine +
                "\tTableName: {TableName}",
                nameof(LegacyContext), Schema.Legacy, MigrationTables.Legacy);
        }
    }
}
