namespace BuildingRegistry.Projections.Wms.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Infrastructure;
    using NetTopologySuite.IO;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Legacy;
    using Legacy.Events;
    using Legacy.Events.Crab;
    using NetTopologySuite.Geometries;

    [ConnectedProjectionName("WMS gebouweenheden")]
    [ConnectedProjectionDescription("Projectie die de gebouweenheden data voor het WMS gebouwregister voorziet.")]
    public class BuildingUnitProjections : ConnectedProjection<WmsContext>
    {
        private readonly WKBReader _wkbReader;

        public BuildingUnitProjections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            #region Building
            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var building =
                    await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.BuildingPersistentLocalId = message.Message.PersistentLocalId;

                foreach (var unit in await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct))
                {
                    unit.BuildingPersistentLocalId = int.Parse(message.Message.PersistentLocalId.ToString());
                }
            });

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                var unitsToRemove = buildingUnits
                    .Where(buildingUnitDetailItem => message.Message.BuildingUnitIds.Contains(buildingUnitDetailItem.BuildingUnitId))
                    .ToList();

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.IsRemoved = true;

                context.BuildingUnits.RemoveRange(unitsToRemove);
            });

            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                foreach (var buildingUnitDetailItem in buildingUnits)
                {
                    buildingUnitDetailItem.Position = null;
                    buildingUnitDetailItem.PositionMethod = null;
                    SetVersion(buildingUnitDetailItem, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                foreach (var unit in buildingUnits)
                {
                    unit.IsBuildingComplete = true;
                    SetVersion(unit, message.Message.Provenance.Timestamp);
                }

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.IsComplete = true;
            });

            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                foreach (var unit in buildingUnits)
                {
                    unit.IsBuildingComplete = false;
                    SetVersion(unit, message.Message.Provenance.Timestamp);
                }

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.IsComplete = false;
            });

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp);

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.Retired;
            });

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp);

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.Retired;
            });

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp);

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.NotRealized;
            });

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp);

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.NotRealized;
            });

            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasRealized>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .BuildingUnitBuildings
                    .AddAsync(new BuildingUnitBuildingItem
                    {
                        BuildingId = message.Message.BuildingId,
                        IsRemoved = false
                    }, ct);
            });
            #endregion Building

            #region BuildingUnit

            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Id = PersistentLocalIdHelper.CreateBuildingUnitId(message.Message.PersistentLocalId);
                buildingUnit.BuildingUnitPersistentLocalId = int.Parse(message.Message.PersistentLocalId.ToString());
            });

            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                await AddBuildingUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, BuildingUnitFunction.Unknown, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) =>
            {
                await AddBuildingUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, BuildingUnitFunction.Unknown, message.Message.Provenance.Timestamp, ct);
                var addedUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                addedUnit.Status = building.BuildingRetiredStatus == BuildingStatus.NotRealized
                    ? BuildingUnitStatus.NotRealized
                    : BuildingUnitStatus.Retired;
            });

            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) =>
            {
                await AddBuildingUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, BuildingUnitFunction.Unknown, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                await AddBuildingUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, BuildingUnitFunction.Common, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                context.BuildingUnits.Remove(buildingUnit);
            });

            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.IsComplete = true;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.IsComplete = false;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            #endregion

            #region BuildingUnitStatus

            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = null;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.NotRealized;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.NotRealized;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.NotRealized;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Planned;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Planned;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Realized;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Realized;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Retired;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Retired;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Retired;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            #endregion BuildingUnitStatus

            #region BuildingPosition

            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                SetPosition(buildingUnit, message.Message.ExtendedWkbGeometry, "AangeduidDoorBeheerder");
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                SetPosition(buildingUnit, message.Message.ExtendedWkbGeometry, "AangeduidDoorBeheerder");
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                SetPosition(buildingUnit, message.Message.ExtendedWkbGeometry, "AfgeleidVanObject");
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                SetPosition(buildingUnit, message.Message.ExtendedWkbGeometry, "AfgeleidVanObject");
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            #endregion

            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.To, cancellationToken: ct);
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.From, cancellationToken: ct);
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasRemoved>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasDuplicated>>(async (context, message, ct) => await DoNothing());

            //CRAB
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingGeometryWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<BuildingStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<HouseNumberWasReaddressedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<SubaddressWasReaddressedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
        }

        private static void SetVersion(BuildingUnit unit, Instant timestamp)
        {
            unit.Version = timestamp;
        }

        private static void RetireUnitsByBuilding(
            IEnumerable<BuildingUnit> buildingUnits,
            ICollection<Guid> buildingUnitIdsToNotRealize,
            ICollection<Guid> buildingUnitIdsToRetire,
            Instant version)
        {
            foreach (var buildingUnitDetailItem in buildingUnits)
            {
                if (buildingUnitIdsToNotRealize.Contains(buildingUnitDetailItem.BuildingUnitId))
                {
                    buildingUnitDetailItem.Status = BuildingUnitStatus.NotRealized;
                }
                else if (buildingUnitIdsToRetire.Contains(buildingUnitDetailItem.BuildingUnitId))
                {
                    buildingUnitDetailItem.Status = BuildingUnitStatus.Retired;
                }

                SetVersion(buildingUnitDetailItem, version);
            }
        }

        private static async Task AddBuildingUnit(
            WmsContext context,
            Guid buildingId,
            Guid buildingUnitId,
            BuildingUnitFunction function,
            Instant version,
            CancellationToken ct)
        {
            var building = await context.BuildingUnitBuildings.FindAsync(buildingId, cancellationToken: ct);

            await context.BuildingUnits.AddAsync(new BuildingUnit
            {
                BuildingId = buildingId,
                BuildingPersistentLocalId = building.BuildingPersistentLocalId,
                BuildingUnitId = buildingUnitId,
                IsBuildingComplete = building.IsComplete ?? false,
                Version = version,
                Function = MapFunction(function)
            }, ct);
        }

        private static string MapFunction(BuildingUnitFunction function)
            => function == BuildingUnitFunction.Common ? "GemeenschappelijkDeel" : "NietGekend";

        private void SetPosition(BuildingUnit buildingUnit, string extendedWkbPosition, string method)
        {
            var geometry = (Point)_wkbReader.Read(extendedWkbPosition.ToByteArray());

            buildingUnit.PositionMethod = method;
            buildingUnit.Position = geometry.AsBinary();
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
