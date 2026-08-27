namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Building;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Operation.Buffer;

    public class BuildingGeometryContext : DbContext, IBuildingGeometries
    {
        private const double AllowedOverlapPercentage = 0.05;

        private readonly Lambert2008ConversionCompletedToggle _conversionCompleted;
        private readonly Lambert2008MatchingReadiness _readiness;

        public DbSet<BuildingGeometryData> BuildingGeometries => Set<BuildingGeometryData>();

        public BuildingGeometryContext()
        {
            _conversionCompleted = new Lambert2008ConversionCompletedToggle(false);
            _readiness = new Lambert2008MatchingReadiness();
        }

        // This needs to be DbContextOptions<T> for Autofac!
        public BuildingGeometryContext(
            DbContextOptions<BuildingGeometryContext> options,
            Lambert2008ConversionCompletedToggle conversionCompleted,
            Lambert2008MatchingReadiness readiness)
            : base(options)
        {
            _conversionCompleted = conversionCompleted;
            _readiness = readiness;
        }

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

            modelBuilder.Entity<BuildingGeometryData>()
                .Property(x => x.SysGeometryLambert2008)
                .HasColumnType("sys.geometry");
        }

        /// <summary>
        /// Brings the incoming outline to the reference system overlap is checked in, and reports which
        /// column holds the stored outlines in that same system. Both matter: SQL Server returns NULL rather
        /// than erroring on an SRID mismatch, so a mismatched check silently finds no overlap at all — and
        /// finding no overlap is what lets an invalid building through. See ADR 0006.
        /// </summary>
        private (Geometry Geometry, int MatchingSrid, bool UseLambert2008) ToMatchingCrs(Geometry geometry)
        {
            var matchingSrid = _conversionCompleted.MatchingSrid;
            var useLambert2008 = matchingSrid == SystemReferenceId.SridLambert2008;

            if (useLambert2008)
            {
                _readiness.EnsureVerified(
                    Lambert2008MatchingReadiness.Buildings,
                    HasIncompleteLambert2008Geometry);
            }

            var matchingGeometry = useLambert2008
                ? geometry.IsLambert08() ? geometry : geometry.EnsureLambert08()
                : geometry.IsLambert72() ? geometry : geometry.EnsureLambert72();

            return (matchingGeometry, matchingSrid, useLambert2008);
        }

        /// <inheritdoc cref="Projections.Legacy.LegacyContext.HasIncompleteLambert2008Geometry"/>
        private bool HasIncompleteLambert2008Geometry()
            => BuildingGeometries
                .AsNoTracking()
                .Any(x => !x.IsRemoved && x.SysGeometry != null && x.SysGeometryLambert2008 == null);

        public ICollection<BuildingGeometryData> GetOverlappingBuildings(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var wkbReader = WKBReaderFactory.CreateForEwkb(extendedWkbGeometry);
            var geometry = wkbReader.Read(extendedWkbGeometry);
            var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(geometry);

            var (matchingGeometry, matchingSrid, useLambert2008) = ToMatchingCrs(fixedGeometry);

            var boundingBox = matchingGeometry.Factory.ToGeometry(matchingGeometry.EnvelopeInternal);
            //check if bounding box is not clockwise otherwise reverse => must be counter clockwise oriented
            if (boundingBox.Coordinates[0].X > boundingBox.Coordinates[1].X)
            {
                boundingBox = boundingBox.Reverse();
            }

            // Two near-identical queries rather than one with a conditional predicate: EF has to translate
            // the column into SQL, so which one is compared cannot be chosen inside it.
            var candidates = useLambert2008
                ? BuildingGeometries
                    .Where(building =>
                        building.BuildingPersistentLocalId != buildingPersistentLocalId
                        && (building.StatusAsString == BuildingStatus.Planned.Value
                            || building.StatusAsString == BuildingStatus.UnderConstruction.Value
                            || building.StatusAsString == BuildingStatus.Realized.Value)
                        && !building.IsRemoved
                        && boundingBox.Intersects(building.SysGeometryLambert2008))
                    .AsEnumerable()
                : BuildingGeometries
                    .Where(building =>
                        building.BuildingPersistentLocalId != buildingPersistentLocalId
                        && (building.StatusAsString == BuildingStatus.Planned.Value
                            || building.StatusAsString == BuildingStatus.UnderConstruction.Value
                            || building.StatusAsString == BuildingStatus.Realized.Value)
                        && !building.IsRemoved
                        && boundingBox.Intersects(building.SysGeometry))
                    .AsEnumerable();

            return candidates
                .Where(building => HasTooMuchOverlap(matchingGeometry, building.SysGeometryIn(matchingSrid)))
                .ToList();
        }

        public ICollection<BuildingGeometryData> GetOverlappingBuildingOutlines(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var wkbReader = WKBReaderFactory.CreateForEwkb(extendedWkbGeometry);
            var geometry = wkbReader.Read(extendedWkbGeometry);
            var fixedGeometry = NetTopologySuite.Geometries.Utilities.GeometryFixer.Fix(geometry);

            var (matchingGeometry, matchingSrid, useLambert2008) = ToMatchingCrs(fixedGeometry);

            var boundingBox = matchingGeometry.Factory.ToGeometry(matchingGeometry.EnvelopeInternal);
            //check if bounding box is not clockwise otherwise reverse => must be counter clockwise oriented
            if (boundingBox.Coordinates[0].X > boundingBox.Coordinates[1].X)
            {
                boundingBox = boundingBox.Reverse();
            }

            var candidates = useLambert2008
                ? BuildingGeometries
                    .Where(building =>
                        building.BuildingPersistentLocalId != buildingPersistentLocalId
                        && building.GeometryMethod == BuildingGeometryMethod.Outlined
                        && building.StatusAsString != BuildingStatus.NotRealized.Value
                        && building.StatusAsString != BuildingStatus.Retired.Value
                        && !building.IsRemoved
                        && boundingBox.Intersects(building.SysGeometryLambert2008))
                    .AsEnumerable()
                : BuildingGeometries
                    .Where(building =>
                        building.BuildingPersistentLocalId != buildingPersistentLocalId
                        && building.GeometryMethod == BuildingGeometryMethod.Outlined
                        && building.StatusAsString != BuildingStatus.NotRealized.Value
                        && building.StatusAsString != BuildingStatus.Retired.Value
                        && !building.IsRemoved
                        && boundingBox.Intersects(building.SysGeometry))
                    .AsEnumerable();

            return candidates
                .Where(building => HasTooMuchOverlap(matchingGeometry, building.SysGeometryIn(matchingSrid)))
                .ToList();
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
