namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda
{
    using Building;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Operation.Buffer;

    public class BuildingGeometryContext : DbContext, IBuildingGeometries
    {
        private const double AllowedOverlapPercentage = 0.05;

        public DbSet<BuildingGeometryData> BuildingGeometries => Set<BuildingGeometryData>();
        public BuildingGeometryContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public BuildingGeometryContext(DbContextOptions<BuildingGeometryContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BuildingGeometryData>()
                .ToTable("BuildingDetailsV2", Schema.Legacy)
                .HasKey(x => x.BuildingPersistentLocalId);

            modelBuilder.Entity<BuildingGeometryData>()
                .Property(x => x.BuildingPersistentLocalId)
                .HasColumnName("PersistentLocalId");

            modelBuilder.Entity<BuildingGeometryData>()
                .Property(x => x.StatusAsString).HasColumnName("Status");

            modelBuilder.Entity<BuildingGeometryData>()
                .Property(x => x.GeometryMethod)
                .HasConversion(
                    x => x.Value,
                    x => BuildingGeometryMethod.Parse(x));

            modelBuilder.Entity<BuildingGeometryData>()
                .Property(x => x.SysGeometry)
                .HasColumnType("sys.geometry");
        }

        public ICollection<BuildingGeometryData> GetOverlappingBuildings(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var wkbReader = WKBReaderFactory.Create();
            var geometry = wkbReader.Read(extendedWkbGeometry);
            var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(geometry);

            var boundingBox = fixedGeometry.Factory.ToGeometry(fixedGeometry.EnvelopeInternal);
            //check if bounding box is not clockwise otherwise reverse => must be counter clockwise oriented
            if (boundingBox.Coordinates[0].X > boundingBox.Coordinates[1].X)
            {
                boundingBox = boundingBox.Reverse();
            }

            var overlappingBuildings = BuildingGeometries
                .Where(building =>
                    building.BuildingPersistentLocalId != buildingPersistentLocalId
                    && (building.StatusAsString == BuildingStatus.Planned.Value
                        || building.StatusAsString == BuildingStatus.UnderConstruction.Value
                        || building.StatusAsString == BuildingStatus.Realized.Value)
                    && !building.IsRemoved
                    && boundingBox.Intersects(building.SysGeometry))
                .AsEnumerable()
                .Where(building => HasTooMuchOverlap(geometry, building.SysGeometry))
                .ToList();

            return overlappingBuildings;
        }

        public ICollection<BuildingGeometryData> GetOverlappingBuildingOutlines(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var wkbReader = WKBReaderFactory.Create();
            var geometry = wkbReader.Read(extendedWkbGeometry);
            var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(geometry);

            var boundingBox = fixedGeometry.Factory.ToGeometry(fixedGeometry.EnvelopeInternal);
            //check if bounding box is not clockwise otherwise reverse => must be counter clockwise oriented
            if (boundingBox.Coordinates[0].X > boundingBox.Coordinates[1].X)
            {
                boundingBox = boundingBox.Reverse();
            }

            var overlappingBuildings = BuildingGeometries
                .Where(building =>
                    building.BuildingPersistentLocalId != buildingPersistentLocalId
                    && building.GeometryMethod == BuildingGeometryMethod.Outlined
                    && building.StatusAsString != BuildingStatus.NotRealized.Value
                    && building.StatusAsString != BuildingStatus.Retired.Value
                    && !building.IsRemoved
                    && boundingBox.Intersects(building.SysGeometry))
                .AsEnumerable()
                .Where(building => HasTooMuchOverlap(geometry, building.SysGeometry))
                .ToList();

            return overlappingBuildings;
        }

        private static bool HasTooMuchOverlap(Geometry newBuildingGeometry, Geometry? existingBuildingGeometry)
        {
            if (existingBuildingGeometry is null)
            {
                return false;
            }

            try
            {
                var overlapArea = newBuildingGeometry.Intersection(existingBuildingGeometry).Area;
                var newBuildingGeometryOverlapPercentage = overlapArea / newBuildingGeometry.Area;
                var existingBuildingGeometryOverlapPercentage = overlapArea / existingBuildingGeometry.Area;

                return newBuildingGeometryOverlapPercentage > AllowedOverlapPercentage
                    || existingBuildingGeometryOverlapPercentage > AllowedOverlapPercentage;
            }
            catch (TopologyException topologyException)
            {
                // Consider buildings that Intersect, but fail with "found non-noded intersection" on calculating, to have an overlap value of 0
                if (topologyException.Message.Contains("found non-noded intersection", StringComparison.InvariantCultureIgnoreCase))
                    return false;

                // any other TopologyException should be treated normally
                throw;
            }
        }
    }
}
