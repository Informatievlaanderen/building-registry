namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetail
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Events;
    using BuildingRegistry.Legacy.Events.Crab;

    [ConnectedProjectionName("API endpoint detail/lijst gebouweenheden")]
    [ConnectedProjectionDescription("Projectie die de gebouweenheden data voor het gebouweenheden detail & lijst voorziet.")]
    public class BuildingUnitDetailProjections : ConnectedProjection<LegacyContext>
    {
        public BuildingUnitDetailProjections()
        {
            #region Building
            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
                {
                    await context
                        .BuildingUnitBuildings
                        .AddAsync(
                            new BuildingUnitBuildingItem
                            {
                                BuildingId = message.Message.BuildingId,
                                IsRemoved = false
                            }, ct);
                });

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                // possible of ALOT of units, might exceed SQL server IN (...) limitation, thats why we first get id by BuildingId
                var buildingUnits = await GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId, ct);

                foreach (var buildingUnitDetailItem in buildingUnits)
                {
                    if (message.Message.BuildingUnitIds.Contains(buildingUnitDetailItem.BuildingUnitId))
                    {
                        buildingUnitDetailItem.IsRemoved = true;
                        var addressesToRemove = context.BuildingUnitAddresses.Local
                            .Where(x => x.BuildingUnitId == buildingUnitDetailItem.BuildingUnitId)
                            .Union(context.BuildingUnitAddresses.Where(x => x.BuildingUnitId == buildingUnitDetailItem.BuildingUnitId)).ToList();
                        context.BuildingUnitAddresses.RemoveRange(addressesToRemove);

                        SetVersion(buildingUnitDetailItem, message.Message.Provenance.Timestamp);
                    }
                }

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.IsRemoved = true;
            });

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                var buildingUnits = await GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp, context);

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.Retired;
            });

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var buildingUnits = await GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp, context);

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.Retired;
            });

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnits = await GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp, context);

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.NotRealized;
            });

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnits = await GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId, ct);
                RetireUnitsByBuilding(buildingUnits, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp, context);

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.NotRealized;
            });

            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) =>
            {
                var buildingUnits = await GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId, ct);
                foreach (var buildingUnit in buildingUnits)
                {
                    buildingUnit.PositionMethod = null;
                    buildingUnit.Position = null;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }
            });

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) =>
            {
                var buildingUnits = await GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId, ct);
                foreach (var buildingUnit in buildingUnits)
                {
                    buildingUnit.IsBuildingComplete = true;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.IsComplete = true;
            });

            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
            {
                var buildingUnits = await GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId, ct);
                foreach (var buildingUnit in buildingUnits)
                {
                    buildingUnit.IsBuildingComplete = false;
                    SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
                }

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.IsComplete = false;
            });

            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingPersistentLocalId = message.Message.PersistentLocalId;

                var buildingUnits = await GetAllBuildingUnitsByBuildingId(context, message.Message.BuildingId, ct);

                foreach (var buildingUnit in buildingUnits)
                    buildingUnit.BuildingPersistentLocalId = message.Message.PersistentLocalId;
            });

            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasRealized>>(async (context, message, ct) => DoNothing());
            #endregion

            //BuildingUnit

            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                if (buildingUnit != null)
                    buildingUnit.PersistentLocalId = message.Message.PersistentLocalId;
            });

            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.IsComplete = true;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.IsComplete = false;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                await AddUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, message.Message.AddressId, message.Message.Provenance.Timestamp, false, ct);
            });

            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) =>
            {
                await AddUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, message.Message.AddressId, message.Message.Provenance.Timestamp, false, ct);
            });

            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                await AddUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, null, message.Message.Provenance.Timestamp, true, ct);
            });

            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) =>
            {
                await AddUnit(context, message.Message.BuildingId, message.Message.BuildingUnitId, null, message.Message.Provenance.Timestamp, false, ct);
                var addedUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                if (building.BuildingRetiredStatus == BuildingStatus.NotRealized)
                    addedUnit.Status = BuildingUnitStatus.NotRealized;
                else
                    addedUnit.Status = BuildingUnitStatus.Retired;
            });

            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                await context.Entry(buildingUnit).Collection(x => x.Addresses).LoadAsync(ct);
                buildingUnit.Addresses.Clear();

                buildingUnit.IsRemoved = true;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails
                    .FindAsync(message.Message.To, cancellationToken: ct);

                await context.Entry(buildingUnit).Collection(x => x.Addresses).LoadAsync(ct);

                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);

                var item = buildingUnit.Addresses.FirstOrDefault(x => x.AddressId == message.Message.AddressId);
                if (item == null)
                {
                    buildingUnit.Addresses.Add(new BuildingUnitDetailAddressItem
                    {
                        AddressId = message.Message.AddressId,
                        BuildingUnitId = message.Message.To,
                        Count = 1
                    });
                }
                else
                {
                    item.Count++;
                }
            });

            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails
                    .FindAsync(message.Message.From, cancellationToken: ct);

                await context.Entry(buildingUnit).Collection(x => x.Addresses).LoadAsync(ct);

                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);

                foreach (var addressId in message.Message.AddressIds)
                {
                    var address = buildingUnit.Addresses.FirstOrDefault(x => x.AddressId == addressId);
                    if (address == null || address.Count <= 1)
                        buildingUnit.Addresses.Remove(address);
                    else
                        address.Count--;
                }
            });

            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails
                    .FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);

                await context.Entry(buildingUnit).Collection(x => x.Addresses).LoadAsync(ct);

                var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressId == message.Message.OldAddressId);
                if (address != null)
                {
                    if (address.Count <= 1)
                        buildingUnit.Addresses.Remove(address);
                    else
                        address.Count = address.Count - 1;

                    buildingUnit.Addresses.Add(new BuildingUnitDetailAddressItem { AddressId = message.Message.NewAddressId, BuildingUnitId = buildingUnit.BuildingUnitId, Count = 1 });
                }
            });

            #region Position

            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                SetGeometry(buildingUnit, message.Message.ExtendedWkbGeometry, BuildingUnitPositionGeometryMethod.AppointedByAdministrator);
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                SetGeometry(buildingUnit, message.Message.ExtendedWkbGeometry, BuildingUnitPositionGeometryMethod.AppointedByAdministrator);
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                SetGeometry(buildingUnit, message.Message.ExtendedWkbGeometry, BuildingUnitPositionGeometryMethod.DerivedFromObject);
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                SetGeometry(buildingUnit, message.Message.ExtendedWkbGeometry, BuildingUnitPositionGeometryMethod.DerivedFromObject);
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            #endregion

            #region Status

            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = null;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Realized;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Retired;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Retired;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.NotRealized;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.NotRealized;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Planned;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Realized;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.NotRealized;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Retired;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                var buildingUnit = await context.BuildingUnitDetails.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                buildingUnit.Status = BuildingUnitStatus.Planned;
                SetVersion(buildingUnit, message.Message.Provenance.Timestamp);
            });

            #endregion

            When<Envelope<BuildingUnitPersistentLocalIdWasDuplicated>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasRemoved>>(async (context, message, ct) => DoNothing());

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

        private static async Task<List<BuildingUnitDetailItem>> GetAllBuildingUnitsByBuildingId(LegacyContext context, Guid buildingId, CancellationToken ct)
        {
            return (await context.BuildingUnitDetails.Where(unit => unit.BuildingId == buildingId).ToListAsync(ct))
                .Union(context.BuildingUnitDetails.Local.Where(unit => unit.BuildingId == buildingId))
                .ToList();
        }

        private static void SetVersion(BuildingUnitDetailItem buildingUnit, Instant timestamp)
        {
            buildingUnit.Version = timestamp;
        }

        private static void RetireUnitsByBuilding(
            IEnumerable<BuildingUnitDetailItem> buildingUnits,
            ICollection<Guid> buildingUnitIdsToNotRealize,
            ICollection<Guid> buildingUnitIdsToRetire,
            Instant version,
            LegacyContext context)
        {
            foreach (var buildingUnitDetailItem in buildingUnits)
            {
                context.Entry(buildingUnitDetailItem).Collection(x => x.Addresses).Load();

                if (buildingUnitIdsToNotRealize.Contains(buildingUnitDetailItem.BuildingUnitId))
                    buildingUnitDetailItem.Status = BuildingUnitStatus.NotRealized;
                else if (buildingUnitIdsToRetire.Contains(buildingUnitDetailItem.BuildingUnitId))
                    buildingUnitDetailItem.Status = BuildingUnitStatus.Retired;

                buildingUnitDetailItem.Addresses.Clear();

                SetVersion(buildingUnitDetailItem, version);
            }
        }

        private static async Task AddUnit(LegacyContext context,
            Guid buildingId,
            Guid buildingUnitId,
            Guid? addressId,
            Instant version,
            bool isCommon,
            CancellationToken ct)
        {
            var building = await context
                .BuildingUnitBuildings
                .FindAsync(buildingId, cancellationToken: ct);

            var buildingUnitDetailItem = new BuildingUnitDetailItem
            {
                BuildingUnitId = buildingUnitId,
                BuildingId = buildingId,
                BuildingPersistentLocalId = building?.BuildingPersistentLocalId,
                IsBuildingComplete = building?.IsComplete ?? false,
                Version = version,
                Function = isCommon ? BuildingUnitFunction.Common : BuildingUnitFunction.Unknown,
            };

            if (addressId.HasValue)
                buildingUnitDetailItem.Addresses.Add(new BuildingUnitDetailAddressItem
                {
                    AddressId = addressId.Value,
                    BuildingUnitId = buildingUnitId,
                    Count = 1
                });

            await context
                .BuildingUnitDetails
                .AddAsync(buildingUnitDetailItem, ct);
        }

        private static void SetGeometry(
            BuildingUnitDetailItem buildingUnit,
            string extendedWkb,
            BuildingUnitPositionGeometryMethod method)
        {
            buildingUnit.PositionMethod = method;
            buildingUnit.Position = extendedWkb.ToByteArray();
        }

        private static void DoNothing() { }
    }
}
