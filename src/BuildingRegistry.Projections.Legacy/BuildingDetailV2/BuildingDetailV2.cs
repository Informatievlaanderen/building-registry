namespace BuildingRegistry.Projections.Legacy.BuildingDetailV2
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using NodaTime;

    public class BuildingDetailItemV2
    {
        public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);

        public int PersistentLocalId { get; set; }
        public BuildingGeometryMethod GeometryMethod { get; set; }
        public byte[] Geometry { get; set; }

        /// <summary>
        /// The outline in Lambert 72 (EPSG 31370). Compared against while
        /// <see cref="Lambert2008ConversionCompletedToggle"/> is off. Null for a building whose geometry is
        /// not a polygon — imported multipolygons — which is why nothing here is matchable.
        /// </summary>
        public Geometry? SysGeometry { get; private set; }

        /// <summary>The same outline in Lambert 2008 (EPSG 3812). Null for rows not written since this column
        /// was added, and for the same non-polygons <see cref="SysGeometry"/> is null for. See ADR 0006.</summary>
        public Geometry? SysGeometryLambert2008 { get; private set; }

        public BuildingStatus Status
        {
            get => BuildingStatus.Parse(StatusAsString);
            set => StatusAsString = value.Value;
        }

        public string StatusAsString { get; private set; }

        public bool IsRemoved { get; set; }

        public DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

        public Instant Version
        {
            get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
            set => VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
        }

        public string? LastEventHash { get; set; }

        private BuildingDetailItemV2()
        {
            Geometry = Array.Empty<byte>();
        }

        public BuildingDetailItemV2(
            int persistentLocalId,
            BuildingGeometryMethod buildingGeometryMethod,
            byte[] geometry,
            Geometry sysGeometry,
            BuildingStatus status,
            bool isRemoved,
            Instant version)
        {
            PersistentLocalId = persistentLocalId;
            GeometryMethod = buildingGeometryMethod;
            Geometry = geometry;
            SetSysGeometry(sysGeometry);
            Status = status;
            IsRemoved = isRemoved;
            Version = version;
        }

        internal void SetGeometry(string extendedWkb, BuildingGeometryMethod geometryMethod)
        {
            Geometry = extendedWkb.ToByteArray();
            GeometryMethod = geometryMethod;
        }

        /// <summary>
        /// Holds the outline in both reference systems, whichever one the event store persisted it in.
        ///
        /// The event store converts to Lambert 2008 gradually, so a single column would hold both systems at
        /// once and neither layer beneath would say so: SQL Server returns NULL on an SRID mismatch rather
        /// than erroring, and the Lambert 72 spatial index's bounding box does not even cover Lambert 2008
        /// coordinates. See ADR 0006.
        ///
        /// A geometry that is not a polygon — imported multipolygons — is stored as null in **both** columns,
        /// as it always has been; it is not matchable in either system.
        ///
        /// One outside Flanders in both systems is refused rather than stored. <c>EnsureLambert08</c> does not
        /// transform such a geometry: it hands **the same instance back** with the SRID overwritten and the
        /// coordinates unmoved, which would leave both columns aliasing one object carrying the wrong label.
        /// </summary>
        public void SetSysGeometry(Geometry? geometry)
        {
            if (geometry is not Polygon)
            {
                SysGeometry = null;
                SysGeometryLambert2008 = null;

                return;
            }

            if (!geometry.IsInsideFlandersUsingLambert72() && !geometry.IsInsideFlandersUsingLambert08())
            {
                throw new InvalidOperationException(
                    $"Building geometry (SRID {geometry.SRID}) lies outside Flanders in both Lambert 72 and "
                    + "Lambert 2008, so it cannot be transformed into either.");
            }

            SysGeometry = geometry.IsLambert72() ? geometry : geometry.EnsureLambert72();
            SysGeometryLambert2008 = geometry.IsLambert08() ? geometry : geometry.EnsureLambert08();
        }

        /// <summary>
        /// Sets only the Lambert 2008 outline, for the building event store's CRS conversion. The building
        /// does not move there, it is re-expressed, so <see cref="SysGeometry"/> is already what it should be
        /// and transforming the payload back would replace it with a round trip of itself. See ADR 0006.
        /// </summary>
        // TODO: Use method when implementing CrsWasChanged event
        public void SetSysGeometryFromCrsConversion(Geometry? geometry)
        {
            if (geometry is not Polygon)
            {
                SysGeometryLambert2008 = null;

                return;
            }

            if (!geometry.IsInsideFlandersUsingLambert72() && !geometry.IsInsideFlandersUsingLambert08())
            {
                throw new InvalidOperationException(
                    $"Building geometry (SRID {geometry.SRID}) lies outside Flanders in both Lambert 72 and "
                    + "Lambert 2008, so it cannot be transformed into either.");
            }

            SysGeometryLambert2008 = geometry.IsLambert08() ? geometry : geometry.EnsureLambert08();
        }

        /// <summary>
        /// The outline in the reference system matching is done in.
        ///
        /// Throws rather than handing back null when the Lambert 2008 column has not been filled for this
        /// building yet, so enabling the toggle too early says so instead of surfacing as a
        /// NullReferenceException further on. A building with no polygon returns null in either system.
        /// </summary>
        public Geometry? SysGeometryIn(int matchingSrid)
        {
            if (matchingSrid != SystemReferenceId.SridLambert2008)
            {
                return SysGeometry;
            }

            if (SysGeometry is null)
            {
                return null;
            }

            return SysGeometryLambert2008 ?? throw new InvalidOperationException(
                $"Building {PersistentLocalId} has no Lambert 2008 geometry, so it cannot be matched in "
                + "Lambert 2008. FeatureToggles:Lambert2008ConversionCompleted must not be enabled before "
                + "the building event store's conversion has filled the column.");
        }
    }

    public class BuildingDetailItemConfiguration : IEntityTypeConfiguration<BuildingDetailItemV2>
    {
        internal const string TableName = "BuildingDetailsV2";
        public static readonly string ProjectionStateName = typeof(BuildingDetailV2Projections).FullName!;

        public void Configure(EntityTypeBuilder<BuildingDetailItemV2> b)
        {
            b.ToTable(TableName, Schema.Legacy)
                .HasKey(p => p.PersistentLocalId)
                .IsClustered();

            b.Property(BuildingDetailItemV2.VersionTimestampBackingPropertyName)
                .HasColumnName("Version");

            b.Ignore(x => x.Version);

            b.Property(p => p.PersistentLocalId)
                .ValueGeneratedNever();
            b.Property(p => p.GeometryMethod)
                .HasConversion(x => x.Value, y => BuildingGeometryMethod.Parse(y));
            b.Property(p => p.Geometry);

            b.Property(p => p.SysGeometry).HasColumnType("sys.geometry");
            b.Property(p => p.SysGeometryLambert2008).HasColumnType("sys.geometry");

            b.Property(p => p.IsRemoved);

            b.Property(x => x.StatusAsString).HasColumnName("Status");
            b.Ignore(p => p.Status);

            b.HasIndex(x => x.IsRemoved);
            b.HasIndex(x => x.StatusAsString);
            b.HasIndex(x => new { x.IsRemoved, x.StatusAsString });
        }
    }
}
