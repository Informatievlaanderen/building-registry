namespace BuildingRegistry.Api.BackOffice.Abstractions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using BuildingRegistry.Building;
    using System.Threading.Tasks;
    using System.Threading;
    using Infrastructure;
    using Microsoft.Data.SqlClient;
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

        public async Task<BuildingUnitBuilding> AddIdempotentBuildingUnitBuilding(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var relation = await BuildingUnitBuildings.FindAsync(new object?[] { buildingUnitPersistentLocalId }, cancellationToken);

            if (relation is not null)
            {
                return relation;
            }

            try
            {
                relation = new BuildingUnitBuilding(buildingUnitPersistentLocalId, buildingPersistentLocalId);
                await BuildingUnitBuildings.AddAsync(relation, cancellationToken);
                await SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                if (exception.InnerException is not SqlException { Number: 2627 })
                {
                    throw;
                }

                relation = await BuildingUnitBuildings.FirstOrDefaultAsync(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId, cancellationToken);

                if (relation is null)
                {
                    throw;
                }
            }

            return relation;
        }

        public async Task RemoveIdempotentBuildingUnitBuildingRelation(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var relation = await BuildingUnitBuildings.FindAsync(new object?[] { (int)buildingUnitPersistentLocalId }, cancellationToken);

            if (relation is null)
            {
                return;
            }

            BuildingUnitBuildings.Remove(relation);
            await SaveChangesAsync(cancellationToken);
        }

        public async Task<BuildingUnitAddressRelation> AddIdempotentBuildingUnitAddressRelation(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var relation = await FindBuildingUnitAddressRelation(buildingUnitPersistentLocalId, addressPersistentLocalId, cancellationToken);
            if (relation is not null)
            {
                return relation;
            }

            try
            {
                relation = new BuildingUnitAddressRelation(buildingPersistentLocalId, buildingUnitPersistentLocalId,
                    addressPersistentLocalId);
                await BuildingUnitAddressRelation.AddAsync(relation, cancellationToken);
                await SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                if (exception.InnerException is not SqlException { Number: 2627 })
                {
                    throw;
                }

                relation = await BuildingUnitAddressRelation.FirstOrDefaultAsync(x => x.AddressPersistentLocalId == addressPersistentLocalId, cancellationToken);

                if (relation is null)
                {
                    throw;
                }
            }

            return relation;
        }

        public async Task RemoveIdempotentBuildingUnitAddressRelation(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var relation = await FindBuildingUnitAddressRelation(buildingUnitPersistentLocalId, addressPersistentLocalId, cancellationToken);

            if (relation is null)
            {
                return;
            }

            BuildingUnitAddressRelation.Remove(relation);
            await SaveChangesAsync(cancellationToken);
        }

        public async Task<BuildingUnitAddressRelation?> FindBuildingUnitAddressRelation(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId, AddressPersistentLocalId addressPersistentLocalId, CancellationToken cancellationToken)
        {
            return await BuildingUnitAddressRelation.FindAsync(new object?[] { (int)buildingUnitPersistentLocalId, (int)addressPersistentLocalId }, cancellationToken);
        }

        public async Task RemoveBuildingUnitAddressRelations(BuildingUnitPersistentLocalId buildingUnitPersistentLocalId, CancellationToken cancellationToken)
        {
            var buildingUnitAddressRelations = await FindAllBuildingUnitAddressRelations(buildingUnitPersistentLocalId, cancellationToken);

            foreach (var buildingUnitAddressRelation in buildingUnitAddressRelations)
            {
                BuildingUnitAddressRelation.Remove(buildingUnitAddressRelation);
            }

            await SaveChangesAsync(cancellationToken);
        }

        public async Task<List<BuildingUnitAddressRelation>> FindAllBuildingUnitAddressRelations(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            CancellationToken cancellationToken)
        {
            return await BuildingUnitAddressRelation
                .Where(x => x.BuildingUnitPersistentLocalId == (int)buildingUnitPersistentLocalId)
                .ToListAsync(cancellationToken);
        }

        public async Task RemoveBuildingUnitAddressRelations(BuildingPersistentLocalId buildingPersistentLocalId, CancellationToken cancellationToken)
        {
            var buildingUnitAddressRelations = await BuildingUnitAddressRelation
                .Where(x => x.BuildingPersistentLocalId == (int)buildingPersistentLocalId)
                .ToListAsync(cancellationToken);

            foreach (var buildingUnitAddressRelation in buildingUnitAddressRelations)
            {
                BuildingUnitAddressRelation.Remove(buildingUnitAddressRelation);
            }

            await SaveChangesAsync(cancellationToken);
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
