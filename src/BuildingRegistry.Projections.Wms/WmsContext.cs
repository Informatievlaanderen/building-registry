namespace BuildingRegistry.Projections.Wms
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IO;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.MigrationExtensions;

    public class WmsContext : RunnerDbContext<WmsContext>
    {
        public override string ProjectionStateSchema => Schema.Wms;

        public DbSet<Building.Building> Buildings { get; set; }
        public DbSet<BuildingUnit.BuildingUnit> BuildingUnits { get; set; }
        public DbSet<BuildingUnit.BuildingUnitBuildingPersistentLocalId> BuildingUnitBuildingPersistentLocalIds { get; set; }

        public WmsContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public WmsContext(DbContextOptions<WmsContext> options)
            : base(options)
        { }
    }

    public class ConfigBasedContextFactory : IDesignTimeDbContextFactory<WmsContext>
    {
        public WmsContext CreateDbContext(string[] args)
        {
            const string migrationConnectionStringName = "WmsProjectionsAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<WmsContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException($"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.Wms, Schema.Wms);
                    sqlServerOptions.UseNetTopologySuite();
                })
                .UseExtendedSqlServerMigrations();

            return new WmsContext(builder.Options);
        }
    }
}
