namespace BuildingRegistry.Projections.Legacy
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using BuildingDetail;
    using BuildingSyndication;
    using BuildingUnitDetail;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IO;

    public class LegacyContext : RunnerDbContext<LegacyContext>
    {
        public override string ProjectionStateSchema => Schema.Legacy;

        public DbSet<BuildingDetailItem> BuildingDetails { get; set; }
        public DbSet<BuildingSyndicationItem> BuildingSyndication { get; set; }
        public DbSet<BuildingUnitDetailItem> BuildingUnitDetails { get; set; }
        public DbSet<BuildingUnitBuildingOsloIdItem> BuildingUnitBuildingOsloIds { get; set; }
        public DbSet<BuildingUnitDetailAddressItem> BuildingUnitAddresses { get; set; }

        public LegacyContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public LegacyContext(DbContextOptions<LegacyContext> options)
            : base(options) { }
    }

    public class ConfigBasedContextFactory : IDesignTimeDbContextFactory<LegacyContext>
    {
        public LegacyContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "LegacyProjectionsAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            var builder = new DbContextOptionsBuilder<LegacyContext>()
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Legacy, Schema.Legacy);
                    sqlServerOptions.UseNetTopologySuite();
                });

            return new LegacyContext(builder.Options);
        }
    }
}
