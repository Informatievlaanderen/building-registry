namespace BuildingRegistry.Projections.Integration
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;
    using BuildingRegistry.Infrastructure;
    using NetTopologySuite.Geometries;

    public class IntegrationContext : RunnerDbContext<IntegrationContext>
    {
        public override string ProjectionStateSchema => Schema.Integration;

        public DbSet<BuildingLatestItem> BuildingLatestItems => Set<BuildingLatestItem>();
        public DbSet<BuildingVersion> BuildingVersions => Set<BuildingVersion>();

        private DbSet<MunicipalityGeometry> MunicipalityGeometries => Set<MunicipalityGeometry>();

        public async Task<string?> FindNiscode(Geometry sysGeometry, CancellationToken ct)
        {
            var municipalityGeometries = await MunicipalityGeometries
                .Where(x => sysGeometry.Within(x.Geometry))
                .ToListAsync(ct);

            // What if multiple municipalities are found? Should we calculate an overlap and order by largest overlap?
            return municipalityGeometries.Count == 1 ? municipalityGeometries.Single().NisCode : null;
        }

        // This needs to be here to please EF
        public IntegrationContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public IntegrationContext(DbContextOptions<IntegrationContext> options)
            : base(options) { }
    }
}
