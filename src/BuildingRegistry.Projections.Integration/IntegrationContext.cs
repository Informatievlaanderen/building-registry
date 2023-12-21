namespace BuildingRegistry.Projections.Integration
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using BuildingRegistry.Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;

    public class IntegrationContext : RunnerDbContext<IntegrationContext>
    {
        public override string ProjectionStateSchema => Schema.Integration;

        public DbSet<BuildingLatestItem> BuildingLatestItems => Set<BuildingLatestItem>();
        public DbSet<BuildingVersion> BuildingVersions => Set<BuildingVersion>();

        public DbSet<BuildingUnitLatestItem> BuildingUnitLatestItems => Set<BuildingUnitLatestItem>();
        public DbSet<BuildingUnitAddress> BuildingUnitAddresses => Set<BuildingUnitAddress>();

        public DbSet<BuildingUnitVersion> BuildingUnitVersions => Set<BuildingUnitVersion>();

        private DbSet<MunicipalityGeometry> MunicipalityGeometries => Set<MunicipalityGeometry>();

        public async Task<string?> FindNiscode(Geometry sysGeometry, CancellationToken ct)
        {
            var municipalityGeometries = await MunicipalityGeometries
                .Where(x => sysGeometry.Within(x.Geometry))
                .ToListAsync(ct);

            // What if multiple municipalities are found? Should we calculate an overlap and order by largest overlap?
            return municipalityGeometries.Count == 1 ? municipalityGeometries.Single().NisCode : null;
        }

        public async Task AddIdempotentBuildingUnitAddress(
            BuildingUnitLatestItem buildingUnit,
            int addressPersistentLocalId,
            CancellationToken ct)
        {
            var buildingUnitAddress = await BuildingUnitAddresses.FindAsync(
                new object[] { buildingUnit.BuildingUnitPersistentLocalId, addressPersistentLocalId }, ct);

            if (buildingUnitAddress is null)
            {
                BuildingUnitAddresses.Add(new BuildingUnitAddress
                {
                    BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                    AddressPersistentLocalId = addressPersistentLocalId
                });
            }
        }

        public async Task RemoveIdempotentBuildingUnitAddress(
            BuildingUnitLatestItem buildingUnit,
            int addressPersistentLocalId,
            CancellationToken ct)
        {
            var buildingUnitAddress = await BuildingUnitAddresses.FindAsync(
                new object[] { buildingUnit.BuildingUnitPersistentLocalId, addressPersistentLocalId }, ct);

            if (buildingUnitAddress is not null)
            {
                BuildingUnitAddresses.Remove(buildingUnitAddress);
            }
        }

        // This needs to be here to please EF
        public IntegrationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public IntegrationContext(DbContextOptions<IntegrationContext> options)
            : base(options) { }
    }
}
