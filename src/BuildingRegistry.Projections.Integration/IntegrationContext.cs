namespace BuildingRegistry.Projections.Integration
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Building.LatestItem;
    using Building.Version;
    using BuildingRegistry.Infrastructure;
    using BuildingUnit.LatestItem;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using BuildingVersionFromMigration = Building.VersionFromMigration.BuildingVersion;

    public class IntegrationContext : RunnerDbContext<IntegrationContext>
    {
        public override string ProjectionStateSchema => Schema.Integration;

        public DbSet<BuildingLatestItem> BuildingLatestItems => Set<BuildingLatestItem>();

        public DbSet<BuildingUnitLatestItem> BuildingUnitLatestItems => Set<BuildingUnitLatestItem>();
        public DbSet<BuildingUnitAddress> BuildingUnitAddresses => Set<BuildingUnitAddress>();

        public DbSet<BuildingVersion> BuildingVersions => Set<BuildingVersion>();
        public DbSet<BuildingVersionFromMigration> BuildingVersionsFromMigration => Set<BuildingVersionFromMigration>();

        private DbSet<MunicipalityGeometry> MunicipalityGeometries => Set<MunicipalityGeometry>();

        public async Task<string?> FindMostIntersectingNisCodeBy(Geometry sysGeometry, CancellationToken ct)
        {
            var municipalityGeometries = await MunicipalityGeometries
                .Where(x => sysGeometry.Intersects(x.Geometry))
                .ToListAsync(ct);

            if (municipalityGeometries.Count > 1)
            {
                return municipalityGeometries
                    .Select(x => new
                    {
                        MunicipalityGeometry = x,
                        Intersection = sysGeometry.Intersection(x.Geometry)
                    }).MaxBy(x => x.Intersection)!
                    .MunicipalityGeometry
                    .NisCode;
            }

            return municipalityGeometries.Count == 1
                ? municipalityGeometries.Single().NisCode
                : null;
        }

        // This needs to be here to please EF
        public IntegrationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public IntegrationContext(DbContextOptions<IntegrationContext> options)
            : base(options) { }
    }
}
