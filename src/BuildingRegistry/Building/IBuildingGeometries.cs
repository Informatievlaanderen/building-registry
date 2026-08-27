namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using NetTopologySuite.Geometries;

    public interface IBuildingGeometries
    {
        ICollection<BuildingGeometryData> GetOverlappingBuildings(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry);

        ICollection<BuildingGeometryData> GetOverlappingBuildingOutlines(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry);
    }

    public sealed class BuildingGeometryData
    {
        public int BuildingPersistentLocalId { get; init; }
        public string StatusAsString { get; init; }
        public BuildingGeometryMethod GeometryMethod { get; init; }

        /// <summary>The outline in Lambert 72 (EPSG 31370).</summary>
        public Geometry SysGeometry { get; init; }

        /// <summary>The same outline in Lambert 2008 (EPSG 3812). See ADR 0006.</summary>
        public Geometry? SysGeometryLambert2008 { get; init; }

        public bool IsRemoved { get; init; }

        private BuildingGeometryData()
        { }

        /// <summary>
        /// Both geometries are taken as given rather than derived from one another: this is a read model of
        /// two columns the projection has already written, and deriving one here would transform a geometry
        /// a second time. <paramref name="sysGeometryLambert2008"/> is null for a row the building event
        /// store's conversion has not reached yet.
        /// </summary>
        public BuildingGeometryData(
            int buildingPersistentLocalId,
            BuildingStatus status,
            BuildingGeometryMethod geometryMethod,
            Geometry sysGeometry,
            bool isRemoved,
            Geometry? sysGeometryLambert2008 = null)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            StatusAsString = status;
            GeometryMethod = geometryMethod;
            SysGeometry = sysGeometry;
            SysGeometryLambert2008 = sysGeometryLambert2008;
            IsRemoved = isRemoved;
        }

        /// <summary>
        /// The outline in the reference system overlap is checked in. Throws rather than returning null when
        /// the Lambert 2008 column has not been filled for this building yet, so enabling the toggle too
        /// early says so instead of silently finding no overlap. See ADR 0006.
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
                $"Building {BuildingPersistentLocalId} has no Lambert 2008 geometry, so its overlap cannot "
                + "be checked in Lambert 2008. FeatureToggles:Lambert2008ConversionCompleted must not be "
                + "enabled before the building event store's conversion has filled the column.");
        }
    }
}
