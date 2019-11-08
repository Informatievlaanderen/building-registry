namespace BuildingRegistry.Projections.Wms.Building
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building.Events;
    using BuildingRegistry.Building.Events.Crab;
    using Infrastructure;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;
    using ValueObjects;

    public class BuildingProjections : ConnectedProjection<WmsContext>
    {
        private readonly WKBReader _wkbReader;

        public BuildingProjections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
            {
                await context.Buildings.AddAsync(new Building { BuildingId = message.Message.BuildingId });
            });

            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Id = PersistentLocalIdHelper.CreateBuildingId(message.Message.PersistentLocalId);
                    building.PersistentLocalId = int.Parse(message.Message.PersistentLocalId.ToString());
                }
            });

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                if (building != null)
                {
                    building.IsComplete = true;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.IsComplete = false;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                context.Buildings.Remove(building);
            });

            When<Envelope<BuildingWasRealized>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Status = BuildingStatus.Realized;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) =>
            {

                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Status = BuildingStatus.Realized;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Status = BuildingStatus.Planned;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Status = BuildingStatus.Planned;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Status = BuildingStatus.Retired;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Status = BuildingStatus.Retired;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Status = BuildingStatus.UnderConstruction;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Status = BuildingStatus.UnderConstruction;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Status = BuildingStatus.NotRealized;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Status = BuildingStatus.NotRealized;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Status = null;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Status = null;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    SetGeometry(building, message.Message.ExtendedWkb, "IngemetenGRB");
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    SetGeometry(building, message.Message.ExtendedWkb, "IngemetenGRB");
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    SetGeometry(building, message.Message.ExtendedWkb, "Ingeschetst");
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    SetGeometry(building, message.Message.ExtendedWkb, "Ingeschetst");
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) =>
            {
                var building = await context.Buildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                if (building != null)
                {
                    building.Geometry = null;
                    building.GeometryMethod = null;
                    SetVersion(building, message.Message.Provenance.Timestamp);
                }
            });

            //BuildingUnit
            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasDuplicated>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) => DoNothing());
            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) => DoNothing());

            //CRAB
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingGeometryWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<HouseNumberWasReaddressedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<SubaddressWasReaddressedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private static void SetVersion(Building building, Instant provenanceTimestamp)
        {
            building.Version = provenanceTimestamp;
        }

        private void SetGeometry(Building building, string extendedWkbGeometry, string method)
        {
            var geometry = (Polygon)_wkbReader.Read(extendedWkbGeometry.ToByteArray());

            building.GeometryMethod = method;
            building.Geometry = geometry.AsBinary();
        }

        private static void DoNothing() { }
    }
}
