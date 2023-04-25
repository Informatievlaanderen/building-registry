namespace BuildingRegistry.Api.Grb.Infrastructure
{
    using System;
    using BuildingRegistry.Grb.Abstractions;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Polly;

    public class MigrationsLogger { }

    public static class MigrationsHelper
    {
        public static void Run(
            string sequenceConnectionString,
            ILoggerFactory loggerFactory = null)
        {
            var logger = loggerFactory?.CreateLogger<MigrationsLogger>();

            Policy
                .Handle<SqlException>()
                .WaitAndRetry(
                    5,
                    retryAttempt =>
                    {
                        var value = Math.Pow(2, retryAttempt) / 4;
                        var randomValue = new Random().Next((int)value * 3, (int)value * 5);
                        logger?.LogInformation("Retrying after {Seconds} seconds...", randomValue);
                        return TimeSpan.FromSeconds(randomValue);
                    })
                .Execute(() =>
                {
                    logger?.LogInformation("Running EF Migrations.");
                    RunBuildingGrb(sequenceConnectionString, loggerFactory);
                });
        }

        private static void RunBuildingGrb(string connectionString, ILoggerFactory? loggerFactory)
        {
            var migratorOptions = new DbContextOptionsBuilder<BuildingGrbContext>()
                .UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                    {
                        sqlServerOptions.EnableRetryOnFailure();
                        sqlServerOptions.MigrationsHistoryTable(BuildingGrbContext.MigrationsTableName, BuildingGrbContext.Schema);
                        sqlServerOptions.UseNetTopologySuite();
                    });

            if (loggerFactory != null)
            {
                migratorOptions = migratorOptions.UseLoggerFactory(loggerFactory);
            }

            using var migrator = new BuildingGrbContext(migratorOptions.Options);
            migrator.Database.Migrate();
        }
    }
}
