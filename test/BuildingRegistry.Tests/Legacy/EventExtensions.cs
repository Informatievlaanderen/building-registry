namespace BuildingRegistry.Tests.Legacy
{
    using System.Collections.Generic;
    using Autofixture;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Crab;
    using BuildingRegistry.Legacy.Events;
    using BuildingRegistry.Legacy.Events.Crab;

    public static class EventExtensions
    {
        public static BuildingUnitWasAdded WithBuildingUnitId(this BuildingUnitWasAdded @event, BuildingUnitId buildingUnitId)
        {
            var newEvent = new BuildingUnitWasAdded(new BuildingId(@event.BuildingId), buildingUnitId, new BuildingUnitKey(@event.BuildingUnitKey), new AddressId(@event.AddressId), new BuildingUnitVersion(@event.BuildingUnitVersion));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static BuildingUnitWasAdded WithBuildingUnitKey(this BuildingUnitWasAdded @event, BuildingUnitKey buildingUnitKey)
        {
            var newEvent = new BuildingUnitWasAdded(new BuildingId(@event.BuildingId), new BuildingUnitId(@event.BuildingUnitId), buildingUnitKey, new AddressId(@event.AddressId), new BuildingUnitVersion(@event.BuildingUnitVersion));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static BuildingUnitWasAdded WithAddressId(this BuildingUnitWasAdded @event, AddressId addressId)
        {
            var newEvent = new BuildingUnitWasAdded(new BuildingId(@event.BuildingId), new BuildingUnitId(@event.BuildingUnitId), new BuildingUnitKey(@event.BuildingUnitKey), addressId, new BuildingUnitVersion(@event.BuildingUnitVersion));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static CommonBuildingUnitWasAdded WithBuildingUnitId(this CommonBuildingUnitWasAdded @event, BuildingUnitId buildingUnitId)
        {
            var newEvent = new CommonBuildingUnitWasAdded(new BuildingId(@event.BuildingId), buildingUnitId, new BuildingUnitKey(@event.BuildingUnitKey), new BuildingUnitVersion(@event.BuildingUnitVersion));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static CommonBuildingUnitWasAdded WithBuildingUnitKey(this CommonBuildingUnitWasAdded @event, BuildingUnitKey buildingUnitKey)
        {
            var newEvent = new CommonBuildingUnitWasAdded(new BuildingId(@event.BuildingId), new BuildingUnitId(@event.BuildingUnitId), buildingUnitKey, new BuildingUnitVersion(@event.BuildingUnitVersion));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static BuildingUnitWasRealized WithBuildingUnitId(this BuildingUnitWasRealized @event, BuildingUnitId buildingUnitId)
        {
            var newEvent = new BuildingUnitWasRealized(new BuildingId(@event.BuildingId), new BuildingUnitId(buildingUnitId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static BuildingUnitWasNotRealized WithBuildingUnitId(this BuildingUnitWasNotRealized @event, BuildingUnitId buildingUnitId)
        {
            var newEvent = new BuildingUnitWasNotRealized(new BuildingId(@event.BuildingId), new BuildingUnitId(buildingUnitId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static BuildingUnitWasRetired WithBuildingUnitId(this BuildingUnitWasRetired @event, BuildingUnitId buildingUnitId)
        {
            var newEvent = new BuildingUnitWasRetired(new BuildingId(@event.BuildingId), new BuildingUnitId(buildingUnitId));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static BuildingWasRemoved WithNoUnits(this BuildingWasRemoved @event)
        {
            var newEvent = new BuildingWasRemoved(new BuildingId(@event.BuildingId), new List<BuildingUnitId>());
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }


        public static BuildingWasRetired WithNoRetiredUnits(this BuildingWasRetired @event)
        {
            var newEvent = new BuildingWasRetired(new BuildingId(@event.BuildingId), new List<BuildingUnitId>(), new List<BuildingUnitId>());
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static BuildingWasNotRealized WithNoRetiredUnits(this BuildingWasNotRealized @event)
        {
            var newEvent = new BuildingWasNotRealized(new BuildingId(@event.BuildingId), new List<BuildingUnitId>(), new List<BuildingUnitId>());
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static BuildingWasMeasuredByGrb WithGeometry(this BuildingWasMeasuredByGrb @event, WkbGeometry geometry, int srid = WkbGeometry.SridLambert72)
        {
            var newEvent = new BuildingWasMeasuredByGrb(new BuildingId(@event.BuildingId), GeometryHelper.CreateEwkbFrom(geometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static BuildingWasOutlined WithGeometry(this BuildingWasOutlined @event, WkbGeometry geometry, int srid = WkbGeometry.SridLambert72)
        {
            var newEvent = new BuildingWasOutlined(new BuildingId(@event.BuildingId), GeometryHelper.CreateEwkbFrom(geometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static BuildingUnitPositionWasDerivedFromObject WithGeometry(this BuildingUnitPositionWasDerivedFromObject @event, WkbGeometry geometry, int srid = WkbGeometry.SridLambert72)
        {
            var newEvent = new BuildingUnitPositionWasDerivedFromObject(new BuildingId(@event.BuildingId), new BuildingUnitId(@event.BuildingUnitId), GeometryHelper.CreateEwkbFrom(geometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static BuildingUnitPositionWasDerivedFromObject WithBuildingUnitId(this BuildingUnitPositionWasDerivedFromObject @event, BuildingUnitId buildingUnitId)
        {
            var newEvent = new BuildingUnitPositionWasDerivedFromObject(new BuildingId(@event.BuildingId), buildingUnitId, new ExtendedWkbGeometry(@event.ExtendedWkbGeometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static BuildingUnitPositionWasAppointedByAdministrator WithGeometry(this BuildingUnitPositionWasAppointedByAdministrator @event, WkbGeometry geometry, int srid = WkbGeometry.SridLambert72)
        {
            var newEvent = new BuildingUnitPositionWasAppointedByAdministrator(new BuildingId(@event.BuildingId), new BuildingUnitId(@event.BuildingUnitId), GeometryHelper.CreateEwkbFrom(geometry));
            ((ISetProvenance)newEvent).SetProvenance(@event.Provenance.ToProvenance());
            return newEvent;
        }

        public static AddressHouseNumberPositionWasImportedFromCrab WithPositionOrigin(
            this AddressHouseNumberPositionWasImportedFromCrab @event, CrabAddressPositionOrigin origin)
        {
            return new AddressHouseNumberPositionWasImportedFromCrab(
                new CrabTerrainObjectId(@event.TerrainObjectId),
                new CrabTerrainObjectHouseNumberId(@event.TerrainObjectHouseNumberId),
                new CrabAddressPositionId(@event.AddressPositionId),
                new CrabHouseNumberId(@event.HouseNumberId),
                new WkbGeometry(@event.AddressPosition),
                origin,
                new CrabAddressNature(@event.AddressNature),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressSubaddressPositionWasImportedFromCrab WithPositionOrigin(
            this AddressSubaddressPositionWasImportedFromCrab @event, CrabAddressPositionOrigin origin)
        {
            return new AddressSubaddressPositionWasImportedFromCrab(
                new CrabTerrainObjectId(@event.TerrainObjectId),
                new CrabTerrainObjectHouseNumberId(@event.TerrainObjectHouseNumberId),
                new CrabAddressPositionId(@event.AddressPositionId),
                new CrabSubaddressId(@event.SubaddressId),
                new WkbGeometry(@event.AddressPosition),
                origin,
                new CrabAddressNature(@event.AddressNature),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }
    }
}
