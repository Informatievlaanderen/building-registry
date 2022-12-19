namespace BuildingRegistry.Api.BackOffice.Abstractions
{
    using System;
    using System.IO;
    using BuildingRegistry.Building;
    using System.Threading.Tasks;
    using System.Threading;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class BackOfficeContext : DbContext
    {
        public BackOfficeContext() { }

        public BackOfficeContext(DbContextOptions<BackOfficeContext> options)
            : base(options) { }

        public DbSet<BuildingUnitBuilding> BuildingUnitBuildings { get; set; }
        public DbSet<BuildingUnitAddressRelation> BuildingUnitAddressRelation { get; set; }


        public async Task<BuildingUnitAddressRelation> AddIdempotentBuildingUnitAddressRelation(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var relation = await BuildingUnitAddressRelation.FindAsync(new object?[] { (int)buildingUnitPersistentLocalId, (int)addressPersistentLocalId }, cancellationToken);

            if (relation is null)
            {
                relation = new BuildingUnitAddressRelation(buildingPersistentLocalId, buildingUnitPersistentLocalId, addressPersistentLocalId);
                await BuildingUnitAddressRelation.AddAsync(relation, cancellationToken);
                await SaveChangesAsync(cancellationToken);
            }

            return relation;
        }

        public async Task RemoveIdempotentParcelAddressRelation(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var relation = await BuildingUnitAddressRelation.FindAsync(new object?[] { (int)buildingUnitPersistentLocalId, (int)addressPersistentLocalId }, cancellationToken);
            if (relation is not null)
            {
                BuildingUnitAddressRelation.Remove(relation);
                await SaveChangesAsync(cancellationToken);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BuildingUnitBuilding>()
                .ToTable("BuildingUnitBuilding", Schema.BackOffice)
                .HasKey(x => x.BuildingUnitPersistentLocalId)
                .IsClustered();

            modelBuilder.Entity<BuildingUnitBuilding>()
                .Property(x => x.BuildingUnitPersistentLocalId)
                .ValueGeneratedNever();

            modelBuilder.Entity<BuildingUnitBuilding>()
                .Property(x => x.BuildingPersistentLocalId);

            modelBuilder.Entity<BuildingUnitAddressRelation>()
                .ToTable("BuildingUnitAddressRelation", Schema.BackOffice)
                .HasKey(x => new { x.BuildingUnitPersistentLocalId, x.AddressPersistentLocalId })
                .IsClustered();

            modelBuilder.Entity<BuildingUnitAddressRelation>()
                .Property(x => x.BuildingPersistentLocalId)
                .ValueGeneratedNever();

            modelBuilder.Entity<BuildingUnitAddressRelation>()
                .HasIndex(x => x.BuildingUnitPersistentLocalId);
            modelBuilder.Entity<BuildingUnitAddressRelation>()
                .Property(x => x.BuildingUnitPersistentLocalId)
                .ValueGeneratedNever();

            modelBuilder.Entity<BuildingUnitAddressRelation>()
                .HasIndex(x => x.AddressPersistentLocalId);
            modelBuilder.Entity<BuildingUnitAddressRelation>()
                .Property(x => x.AddressPersistentLocalId)
                .ValueGeneratedNever();
        }
    }

    public class BuildingUnitBuilding
    {
        public int BuildingUnitPersistentLocalId { get; set; }
        public int BuildingPersistentLocalId { get; set; }

        private BuildingUnitBuilding() { }

        public BuildingUnitBuilding(
            int buildingUnitPersistentLocalId,
            int buildingPersistentLocalId)
        {
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            BuildingPersistentLocalId = buildingPersistentLocalId;
        }
    }

    public class BuildingUnitAddressRelation
    {
        public int BuildingPersistentLocalId { get; set; }
        public int BuildingUnitPersistentLocalId { get; set; }
        public int AddressPersistentLocalId { get; set; }

        private BuildingUnitAddressRelation()
        { }

        public BuildingUnitAddressRelation(int buildingPersistentLocalId,int buildingUnitPersistentLocal, int addressPersistentLocalId)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocal;
            AddressPersistentLocalId = addressPersistentLocalId;
        }
    }

    public class ConfigBasedSequenceContextFactory : IDesignTimeDbContextFactory<BackOfficeContext>
    {
        public BackOfficeContext CreateDbContext(string[] args)
        {
            var migrationConnectionStringName = "BackOffice";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true)
                .AddEnvironmentVariables()
                .Build();

            var builder = new DbContextOptionsBuilder<BackOfficeContext>();

            var connectionString = configuration.GetConnectionString(migrationConnectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException(
                    $"Could not find a connection string with name '{migrationConnectionStringName}'");

            builder
                .UseSqlServer(connectionString, sqlServerOptions =>
                {
                    sqlServerOptions.EnableRetryOnFailure();
                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice);
                });

            return new BackOfficeContext(builder.Options);
        }
    }
}
