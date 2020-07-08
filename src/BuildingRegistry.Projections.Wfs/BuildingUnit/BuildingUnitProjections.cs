namespace BuildingRegistry.Projections.Wfs.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building.Events;
    using Infrastructure;
    using NetTopologySuite.IO;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using NetTopologySuite.Geometries;
    using ValueObjects;

    public class BuildingUnitProjections : ConnectedProjection<WfsContext>
    {
        private readonly WKBReader _wkbReader;
        private static readonly string NotRealizedStatus = GebouweenheidStatus.NietGerealiseerd.ToString();
        private static readonly string RetiredStatus = GebouweenheidStatus.Gehistoreerd.ToString();
        private static readonly string PlannedStatus = GebouweenheidStatus.Gepland.ToString();
        private static readonly string RealizedStatus = GebouweenheidStatus.Gerealiseerd.ToString();
        private static readonly string AppointedByAdministratorMethod = PositieGeometrieMethode.AangeduidDoorBeheerder.ToString();
        private static readonly string DerivedFromObjectMethod = PositieGeometrieMethode.AfgeleidVanObject.ToString();

        public BuildingUnitProjections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            #region Building
            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var building =
                    await context.BuildingUnitsBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.BuildingPersistentLocalId = message.Message.PersistentLocalId;

                foreach (var unit in await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct))
                {
                    unit.BuildingPersistentLocalId = message.Message.PersistentLocalId;
                }
            });

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                foreach (var buildingUnitDetailItem in buildingUnits)
                {
                    if (message.Message.BuildingUnitIds.Contains(buildingUnitDetailItem.BuildingUnitId))
                    {
                        buildingUnitDetailItem.IsRemoved = true;
                        SetVersion(buildingUnitDetailItem, message.Message.Provenance.Timestamp);
                    }
                }

                var building = await context.BuildingUnitsBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.IsRemoved = true;
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

                var building = await context.BuildingUnitsBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
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

                var building = await context.BuildingUnitsBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.IsComplete = false;
            });

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp);

                var building = await context.BuildingUnitsBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.Retired;
            });

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp);

                var building = await context.BuildingUnitsBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.Retired;
            });

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp);

                var building = await context.BuildingUnitsBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.NotRealized;
            });

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp);

                var building = await context.BuildingUnitsBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.NotRealized;
            });

            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .BuildingUnitsBuildings
                    .AddAsync(new BuildingUnitBuildingItem
                    {
                        BuildingId = message.Message.BuildingId,
                        IsRemoved = false,
                    }, ct);
            });
            #endregion Building

            #region BuildingUnit

            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Id = PersistentLocalIdHelper.CreateBuildingUnitId(message.Message.PersistentLocalId);
                    buildingUnit.BuildingUnitPersistentLocalId = message.Message.PersistentLocalId;
                }
            });

            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                await AddBuildingUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, BuildingUnitFunction.Unknown, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) =>
            {
                await AddBuildingUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, BuildingUnitFunction.Unknown, message.Message.Provenance.Timestamp, ct);
                var addedUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                var building = await context.BuildingUnitsBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                if (building.BuildingRetiredStatus == BuildingStatus.NotRealized)
                    addedUnit.Status = NotRealizedStatus;
                else
                    addedUnit.Status = RetiredStatus;
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
                buildingUnit.IsRemoved = true;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.IsComplete = true;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.IsComplete = false;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            #endregion

            #region BuildingUnitStatus

            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = null;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = NotRealizedStatus;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = NotRealizedStatus;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = NotRealizedStatus;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = PlannedStatus;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = PlannedStatus;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = RealizedStatus;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = RealizedStatus;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = RetiredStatus;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = RetiredStatus;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = RetiredStatus;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            #endregion BuildingUnitStatus

            #region BuildingPosition

            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    SetPosition(buildingUnit, message.Message.Position, AppointedByAdministratorMethod);
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    SetPosition(buildingUnit, message.Message.Position, AppointedByAdministratorMethod);
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    SetPosition(buildingUnit, message.Message.Position, DerivedFromObjectMethod);
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    SetPosition(buildingUnit, message.Message.Position, DerivedFromObjectMethod);
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
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
                    buildingUnitDetailItem.Status = NotRealizedStatus;
                else if (buildingUnitIdsToRetire.Contains(buildingUnitDetailItem.BuildingUnitId))
                    buildingUnitDetailItem.Status = RetiredStatus;

                SetVersion(buildingUnitDetailItem, version);
            }
        }

        private static async Task AddBuildingUnit(
            WfsContext context,
            Guid buildingId,
            Guid buildingUnitId,
            BuildingUnitFunction function,
            Instant version,
            CancellationToken ct)
        {
            var building = await context.BuildingUnitsBuildings.FindAsync(buildingId, cancellationToken: ct);

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
            => function == BuildingUnitFunction.Common ? GebouweenheidFunctie.GemeenschappelijkDeel.ToString() : GebouweenheidFunctie.NietGekend.ToString();

        private void SetPosition(BuildingUnit buildingUnit, string extendedWkbPosition, string method)
        {
            var geometry = (Point)_wkbReader.Read(extendedWkbPosition.ToByteArray());

            buildingUnit.PositionMethod = method;
            buildingUnit.Position = geometry;
        }
    }
}
