namespace BuildingRegistry.Building
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Events;
    using Exceptions;
    using NetTopologySuite.Geometries;

    public partial class Building : AggregateRootEntity
    {
        public static readonly Func<Building> Factory = () => new Building();

        public static Building MigrateBuilding(
            BuildingId buildingId,
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingPersistentLocalIdAssignmentDate assignmentDate,
            BuildingStatus buildingStatus,
            BuildingGeometry buildingGeometry,
            bool isRemoved,
            List<Commands.BuildingUnit> buildingUnits)
        {
            var newBuilding = Factory();
            newBuilding.ApplyChange(new BuildingWasMigrated(
                buildingId,
                buildingPersistentLocalId,
                assignmentDate,
                buildingStatus,
                buildingGeometry,
                isRemoved,
                buildingUnits));

            return newBuilding;
        }

        public static Building Plan(
            BuildingPersistentLocalId buildingPersistentLocalId,
            ExtendedWkbGeometry extendedWkbGeometry)
        {
            var geometry = WKBReaderFactory.Create().Read(extendedWkbGeometry);

            GuardPolygon(geometry);

            var newBuilding = Factory();
            newBuilding.ApplyChange(new BuildingWasPlannedV2(
                buildingPersistentLocalId,
                extendedWkbGeometry));

            return newBuilding;
        }

        private static void GuardPolygon(Geometry? geometry)
        {
            if (
                geometry == null
                || geometry is not Polygon
                || geometry.SRID != ExtendedWkbGeometry.SridLambert72
                || !GeometryValidator.IsValid(geometry))
            {
                throw new InvalidPolygonException();
            }
        }

        #region Metadata
        protected override void BeforeApplyChange(object @event)
        {
            new EventMetadataContext(new Dictionary<string, object>());
            base.BeforeApplyChange(@event);
        }
        #endregion
    }
}
