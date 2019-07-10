namespace BuildingRegistry.Projections.Wms.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building.Events;
    using GeoAPI.Geometries;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.IO;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using ValueObjects;

    public class BuildingUnitProjections : ConnectedProjection<WmsContext>
    {
        private readonly WKBReader _wkbReader;

        public BuildingUnitProjections(WKBReader wkbReader)
        {
            _wkbReader = wkbReader;

            #region Building
            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.BuildingUnitBuildingPersistentLocalIds.AddAsync(new BuildingUnitBuildingPersistentLocalId
                {
                    BuildingId = message.Message.BuildingId,
                    BuildingPersistentLocalId = message.Message.PersistentLocalId.ToString()
                }, ct);

                foreach (var unit in await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct))
                {
                    unit.BuildingPersistentLocalId = int.Parse(message.Message.PersistentLocalId.ToString());
                }
            });

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                var unitsToRemove = new List<BuildingUnit>();
                foreach (var buildingUnitDetailItem in buildingUnits)
                {
                    if (message.Message.BuildingUnitIds.Contains(buildingUnitDetailItem.BuildingUnitId))
                        unitsToRemove.Add(buildingUnitDetailItem);
                }

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
            });

            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                foreach (var unit in buildingUnits)
                {
                    unit.IsBuildingComplete = false;
                    SetVersion(unit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnits = await context.BuildingUnits.GetByBuildingId(message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp);
            });
            #endregion Building

            #region BuildingUnit

            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Id = PersistentLocalIdHelper.CreateBuildingUnitId(message.Message.PersistentLocalId);
                    buildingUnit.BuildingUnitPersistentLocalId = int.Parse(message.Message.PersistentLocalId.ToString());
                }
            });

            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                await AddBuildingUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, BuildingUnitFunction.Unknown, message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) =>
            {
                await AddBuildingUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, BuildingUnitFunction.Unknown, message.Message.Provenance.Timestamp, ct);
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
                    buildingUnit.Status = BuildingUnitStatus.NotRealized;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = BuildingUnitStatus.NotRealized;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = BuildingUnitStatus.NotRealized;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasNotRealizedByBuilding>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = BuildingUnitStatus.NotRealized;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = BuildingUnitStatus.Planned;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = BuildingUnitStatus.Planned;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = BuildingUnitStatus.Realized;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = BuildingUnitStatus.Realized;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = BuildingUnitStatus.Retired;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = BuildingUnitStatus.Retired;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    buildingUnit.Status = BuildingUnitStatus.Retired;
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
                    SetPosition(buildingUnit, message.Message.Position, "AangeduidDoorBeheerder");
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    SetPosition(buildingUnit, message.Message.Position, "AangeduidDoorBeheerder");
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    SetPosition(buildingUnit, message.Message.Position, "AfgeleidVanObject");
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnits.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                {
                    SetPosition(buildingUnit, message.Message.Position, "AfgeleidVanObject");
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            #endregion
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
                    buildingUnitDetailItem.Status = BuildingUnitStatus.NotRealized;
                else if (buildingUnitIdsToRetire.Contains(buildingUnitDetailItem.BuildingUnitId))
                    buildingUnitDetailItem.Status = BuildingUnitStatus.Retired;

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
            var buildingPersistentLocalId = await context.BuildingUnitBuildingPersistentLocalIds.FindAsync(buildingId, cancellationToken: ct);

            await context.BuildingUnits.AddAsync(new BuildingUnit
            {
                BuildingId = buildingId,
                BuildingPersistentLocalId = buildingPersistentLocalId == null ? null : (int?)int.Parse(buildingPersistentLocalId.BuildingPersistentLocalId),
                BuildingUnitId = buildingUnitId,
                Version = version,
                Function = MapFunction(function)
            }, ct);
        }

        private static string MapFunction(BuildingUnitFunction function)
        {
            if (function == BuildingUnitFunction.Common)
                return "GemeenschappelijkDeel";
            return "NietGekend";
        }

        private void SetPosition(BuildingUnit buildingUnit, string extendedWkbPosition, string method)
        {
            var geometry = (IPoint)_wkbReader.Read(extendedWkbPosition.ToByteArray());

            buildingUnit.PositionMethod = method;
            buildingUnit.Position = geometry;
        }
    }
}
