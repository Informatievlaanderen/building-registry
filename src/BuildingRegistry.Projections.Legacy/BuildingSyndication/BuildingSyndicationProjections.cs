namespace BuildingRegistry.Projections.Legacy.BuildingSyndication
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building.Events;
    using NodaTime;
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using ValueObjects;
    using Building.Events.Crab;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class BuildingSyndicationProjections : ConnectedProjection<LegacyContext>
    {
        public BuildingSyndicationProjections()
        {
            #region Building Events

            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
            {
                var newBuildingSyndicationItem = new BuildingSyndicationItem
                {
                    Position = message.Position,
                    BuildingId = message.Message.BuildingId,
                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                };

                newBuildingSyndicationItem.ApplyProvenance(message.Message.Provenance);
                newBuildingSyndicationItem.SetEventData(message.Message);

                await context
                    .BuildingSyndication
                    .AddAsync(newBuildingSyndicationItem, ct);
            });

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => x.IsComplete = true,
                    ct);
            });

            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => x.IsComplete = false,
                    ct);
            });

            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => x.Status = BuildingStatus.UnderConstruction,
                    ct);
            });

            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        x.GeometryMethod = null;
                        x.Geometry = null;
                    },
                    ct);
            });

            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        x.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb;
                        x.Geometry = message.Message.ExtendedWkb.ToByteArray();
                    },
                    ct);
            });

            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => x.PersistentLocalId = message.Message.PersistentLocalId,
                    ct);
            });

            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        x.GeometryMethod = BuildingGeometryMethod.Outlined;
                        x.Geometry = message.Message.ExtendedWkb.ToByteArray();
                    },
                    ct);
            });

            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => x.Status = null,
                    ct);
            });

            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => x.Status = null,
                    ct);
            });

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        x.Status = BuildingStatus.NotRealized;

                        RetireUnitsByBuilding(
                            x.BuildingUnits,
                            message.Message.BuildingUnitIdsToNotRealize,
                            message.Message.BuildingUnitIdsToRetire,
                            message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => x.Status = BuildingStatus.Planned,
                    ct);
            });

            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => x.Status = BuildingStatus.Realized,
                    ct);
            });

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        x.Status = BuildingStatus.Retired;

                        RetireUnitsByBuilding(
                            x.BuildingUnits,
                            message.Message.BuildingUnitIdsToNotRealize,
                            message.Message.BuildingUnitIdsToRetire,
                            message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => x.Status = BuildingStatus.UnderConstruction,
                    ct);
            });

            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        x.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb;
                        x.Geometry = message.Message.ExtendedWkb.ToByteArray();
                    },
                    ct);
            });

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        x.Status = BuildingStatus.NotRealized;

                        RetireUnitsByBuilding(
                            x.BuildingUnits,
                            message.Message.BuildingUnitIdsToNotRealize,
                            message.Message.BuildingUnitIdsToRetire,
                            message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        x.GeometryMethod = BuildingGeometryMethod.Outlined;
                        x.Geometry = message.Message.ExtendedWkb.ToByteArray();
                    },
                    ct);
            });

            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => x.Status = BuildingStatus.Planned,
                    ct);
            });

            When<Envelope<BuildingWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => x.Status = BuildingStatus.Realized,
                    ct);
            });

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        x.Status = BuildingStatus.Retired;

                        RetireUnitsByBuilding(
                            x.BuildingUnits,
                            message.Message.BuildingUnitIdsToNotRealize,
                            message.Message.BuildingUnitIdsToRetire,
                            message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => { },
                    ct);
            });

            #endregion Building Events

            #region Building Unit Events

            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var buildingUnitSyndicationItem = new BuildingUnitSyndicationItem
                        {
                            Position = message.Position,
                            BuildingUnitId = message.Message.BuildingUnitId,
                            Function = BuildingUnitFunction.Common,
                            Version = message.Message.Provenance.Timestamp
                        };

                        x.BuildingUnits.Add(buildingUnitSyndicationItem);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var buildingUnitSyndicationItem = new BuildingUnitSyndicationItem
                        {
                            Position = message.Position,
                            BuildingUnitId = message.Message.BuildingUnitId,
                            Function = BuildingUnitFunction.Unknown,
                            Version = message.Message.Provenance.Timestamp,
                            Addresses =
                            {
                                new BuildingUnitAddressSyndicationItem
                                {
                                    BuildingUnitId = message.Message.BuildingUnitId,
                                    Position = message.Position,
                                    AddressId = message.Message.AddressId,
                                    Count = 1
                                }
                            }
                        };

                        x.BuildingUnits.Add(buildingUnitSyndicationItem);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    (oldSyndicationItem, newSyndicationItem) =>
                    {
                        var buildingUnitSyndicationItem = new BuildingUnitSyndicationItem
                        {
                            Position = message.Position,
                            BuildingUnitId = message.Message.BuildingUnitId,
                            Function = BuildingUnitFunction.Unknown,
                            Version = message.Message.Provenance.Timestamp,
                            Status = oldSyndicationItem.Status == BuildingStatus.NotRealized
                                ? BuildingUnitStatus.NotRealized
                                : BuildingUnitStatus.Retired
                        };

                        buildingUnitSyndicationItem.Addresses.Add(new BuildingUnitAddressSyndicationItem
                        {
                            BuildingUnitId = message.Message.BuildingUnitId,
                            Position = message.Position,
                            AddressId = message.Message.AddressId,
                            Count = 1
                        });

                        newSyndicationItem.BuildingUnits.Add(buildingUnitSyndicationItem);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var buildingUnitSyndicationItem = new BuildingUnitSyndicationItem
                        {
                            Position = message.Position,
                            BuildingUnitId = message.Message.BuildingUnitId,
                            Function = BuildingUnitFunction.Unknown,
                            Version = message.Message.Provenance.Timestamp,
                            Addresses =
                            {
                                new BuildingUnitAddressSyndicationItem
                                {
                                    BuildingUnitId = message.Message.BuildingUnitId,
                                    Position = message.Position,
                                    AddressId = message.Message.AddressId,
                                    Count = 1
                                }
                            }
                        };

                        x.BuildingUnits.Add(buildingUnitSyndicationItem);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => x.BuildingUnits.Remove(x.BuildingUnits.FirstOrDefault(y => y.BuildingUnitId == message.Message.BuildingUnitId)),
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(u => u.BuildingUnitId == message.Message.To);

                        if (unit.Addresses.Any(u => u.AddressId == message.Message.AddressId))
                        {
                            var address = unit.Addresses.Single(u => u.AddressId == message.Message.AddressId);
                            address.Count += 1;
                        }
                        else
                        {
                            unit.Addresses.Add(new BuildingUnitAddressSyndicationItem
                            {
                                AddressId = message.Message.AddressId,
                                BuildingUnitId = unit.BuildingUnitId,
                                Count = 1,
                                Position = message.Position,
                            });
                        }

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(u => u.BuildingUnitId == message.Message.From);

                        foreach (var addressId in message.Message.AddressIds)
                        {
                            var addressSyndicationItem = unit.Addresses.SingleOrDefault(u => u.AddressId == addressId);
                            if (addressSyndicationItem != null && addressSyndicationItem.Count > 1)
                                addressSyndicationItem.Count -= 1;
                            else
                                unit.Addresses.Remove(addressSyndicationItem);
                        }

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.IsComplete = true;

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.IsComplete = false;

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.SingleOrDefault(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        if (unit != null)
                            unit.PersistentLocalId = message.Message.PersistentLocalId;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.PositionMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator;
                        unit.PointPosition = message.Message.Position.ToByteArray();

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                        unit.PointPosition = message.Message.Position.ToByteArray();

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                        unit.PointPosition = message.Message.Position.ToByteArray();

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.PositionMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator;
                        unit.PointPosition = message.Message.Position.ToByteArray();

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.Status = null;

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.Status = BuildingUnitStatus.NotRealized;

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.Status = BuildingUnitStatus.Planned;

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.Status = BuildingUnitStatus.Realized;

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.Status = BuildingUnitStatus.Retired;

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);
                        unit.Status = BuildingUnitStatus.NotRealized;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.Status = BuildingUnitStatus.NotRealized;

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.Status = BuildingUnitStatus.Planned;

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.Status = BuildingUnitStatus.Realized;

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.Status = BuildingUnitStatus.Retired;

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        var unit = x.BuildingUnits.Single(y => y.BuildingUnitId == message.Message.BuildingUnitId);

                        unit.Status = BuildingUnitStatus.Retired;

                        ApplyUnitVersion(unit, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x =>
                    {
                        x.LastChangedOn = Instant.FromDateTimeUtc(message.Message.BeginDate.AtStartOfDayInZone(DateTimeZone.Utc).ToDateTimeUtc());

                        x.BuildingUnits
                            .Single(y => y.BuildingUnitId == message.Message.BuildingUnitId)
                            .Readdresses
                            .Add(new BuildingUnitReaddressSyndicationItem
                            {
                                Position = message.Position,
                                BuildingUnitId = message.Message.BuildingUnitId,
                                OldAddressId = message.Message.OldAddressId,
                                NewAddressId = message.Message.NewAddressId,
                                ReaddressBeginDate = message.Message.BeginDate
                            });
                    },
                    ct);
            });

            When<Envelope<BuildingUnitPersistentLocalIdWasDuplicated>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => { },
                    ct);
            });

            When<Envelope<BuildingUnitPersistentLocalIdWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingId,
                    message,
                    x => { },
                    ct);
            });

            #endregion

            //CRAB
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(DoNothing);
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(DoNothing);
            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(DoNothing);
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(DoNothing);
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(DoNothing);
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(DoNothing);
            When<Envelope<BuildingGeometryWasImportedFromCrab>>(DoNothing);
            When<Envelope<BuildingStatusWasImportedFromCrab>>(DoNothing);
            When<Envelope<HouseNumberWasReaddressedFromCrab>>(DoNothing);
            When<Envelope<SubaddressWasReaddressedFromCrab>>(DoNothing);
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(DoNothing);
            When<Envelope<TerrainObjectWasImportedFromCrab>>(DoNothing);
        }

        private static void ApplyUnitVersion(BuildingUnitSyndicationItem item, Instant version)
        {
            item.Version = version;
        }

        private static void RetireUnitsByBuilding(
            IEnumerable<BuildingUnitSyndicationItem> buildingUnits,
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

                buildingUnitDetailItem.Addresses.Clear();

                ApplyUnitVersion(buildingUnitDetailItem, version);
            }
        }

        private static Task DoNothing<T>(LegacyContext _, Envelope<T> __) => Task.CompletedTask;
    }
}
