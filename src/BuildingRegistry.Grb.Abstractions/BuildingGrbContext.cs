namespace BuildingRegistry.Grb.Abstractions
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class BuildingGrbContext : DbContext
    {
        public const string Schema = "BuildingRegistryGrb";
        public const string MigrationsTableName = "__EFMigrationsHistoryBuildingGrb";

        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<JobRecord> JobRecords => Set<JobRecord>();
        public DbSet<JobResult> JobResults => Set<JobResult>();

        public BuildingGrbContext() { }

        public BuildingGrbContext(DbContextOptions<BuildingGrbContext> options)
            : base(options)
        { }

        public async Task<Job?> FindJob(Guid jobId, CancellationToken cancellationToken)
        {
            return await Jobs.FindAsync(new object[] { jobId }, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BuildingGrbContext).Assembly);
        }
    }

    public class ConfigBasedBuildingGrbContextFactory : IDesignTimeDbContextFactory<BuildingGrbContext>
    {
        public BuildingGrbContext CreateDbContext(string[] args)
        {
            var migrationConnectionStringName = "BuildingGrbAdmin";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<BuildingGrbContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(
                    $"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(BuildingGrbContext.MigrationsTableName, BuildingGrbContext.Schema);
                    sqlServerOptions.UseNetTopologySuite();
                });

            return new BuildingGrbContext(builder.Options);
        }
    }
}
