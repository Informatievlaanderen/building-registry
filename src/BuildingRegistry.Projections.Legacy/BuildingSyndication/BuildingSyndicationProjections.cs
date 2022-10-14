namespace BuildingRegistry.Projections.Legacy.BuildingSyndication
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building.Events;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Events;
    using BuildingRegistry.Legacy.Events.Crab;
    using NodaTime;

    [ConnectedProjectionName("Feed endpoint gebouwen")]
    [ConnectedProjectionDescription("Projectie die de gebouwen- en gebouweenheden data voor de gebouwen feed voorziet.")]
    public class BuildingSyndicationProjections : ConnectedProjection<LegacyContext>
    {
        public BuildingSyndicationProjections()
        {
            #region Legacy
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
                    SyndicationItemCreatedAt = DateTimeOffset.Now
                };

                newBuildingSyndicationItem.ApplyProvenance(message.Message.Provenance);
                newBuildingSyndicationItem.SetEventData(message.Message, message.EventName);

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
                        x.Geometry = message.Message.ExtendedWkbGeometry.ToByteArray();
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
                        x.Geometry = message.Message.ExtendedWkbGeometry.ToByteArray();
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
                        x.Geometry = message.Message.ExtendedWkbGeometry.ToByteArray();
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
                        x.Geometry = message.Message.ExtendedWkbGeometry.ToByteArray();
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
                                Position = message.Position
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
                            if (addressSyndicationItem is { Count: > 1 })
                            {
                                addressSyndicationItem.Count -= 1;
                            }
                            else if (addressSyndicationItem is not null)
                            {
                                unit.Addresses.Remove(addressSyndicationItem);
                            }
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
                        {
                            unit.PersistentLocalId = message.Message.PersistentLocalId;
                        }
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
                        unit.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();

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
                        unit.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();

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
                        unit.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();

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
                        unit.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();

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

            #endregion Legacy

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var buildingUnitSyndicationItemsV2 = new Collection<BuildingUnitSyndicationItemV2>();
                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    var addressesIdsGrouped = buildingUnit.AddressPersistentLocalIds.GroupBy(x => x);
                    var addresses = addressesIdsGrouped
                        .Select(groupedAddressId => new BuildingUnitAddressSyndicationItemV2
                        {
                            Position = message.Position,
                            BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                            AddressPersistentLocalId = groupedAddressId.Key,
                            Count = groupedAddressId.Count()
                        })
                        .ToList();

                    buildingUnitSyndicationItemsV2.Add(new BuildingUnitSyndicationItemV2
                    {
                        Position = message.Position,
                        PersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                        Status = BuildingRegistry.Building.BuildingUnitStatus.Parse(buildingUnit.Status),
                        Function = BuildingRegistry.Building.BuildingUnitFunction.Parse(buildingUnit.Function),
                        PositionMethod = BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod),
                        PointPosition = buildingUnit.ExtendedWkbGeometry.ToByteArray(),
                        Version = message.Message.Provenance.Timestamp,
                        Addresses = new Collection<BuildingUnitAddressSyndicationItemV2>(addresses)
                    });
                }

                var newBuildingSyndicationItem = new BuildingSyndicationItem
                {
                    Position = message.Position,
                    BuildingId = null, //while we have the information, we shouldn't identify this resource with its old guid id
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Status = MapBuildingStatus(BuildingRegistry.Building.BuildingStatus.Parse(message.Message.BuildingStatus)),
                    GeometryMethod = MapBuildingGeometryMethod(BuildingRegistry.Building.BuildingGeometryMethod.Parse(message.Message.GeometryMethod)),
                    Geometry = message.Message.ExtendedWkbGeometry.ToByteArray(),
                    IsComplete = true,
                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.Now,
                    BuildingUnitsV2 = buildingUnitSyndicationItemsV2
                };

                newBuildingSyndicationItem.ApplyProvenance(message.Message.Provenance);
                newBuildingSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .BuildingSyndication
                    .AddAsync(newBuildingSyndicationItem, ct);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingUnitSyndicationItems = new Collection<BuildingUnitSyndicationItemV2>();
                var newBuildingSyndicationItem = new BuildingSyndicationItem
                {
                    Position = message.Position,
                    PersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Status = MapBuildingStatus(BuildingRegistry.Building.BuildingStatus.Planned),
                    GeometryMethod = MapBuildingGeometryMethod(BuildingRegistry.Building.BuildingGeometryMethod.Outlined),
                    Geometry = message.Message.ExtendedWkbGeometry.ToByteArray(),
                    IsComplete = true,
                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.Now,
                    BuildingUnitsV2 = buildingUnitSyndicationItems
                };

                newBuildingSyndicationItem.ApplyProvenance(message.Message.Provenance);
                newBuildingSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .BuildingSyndication
                    .AddAsync(newBuildingSyndicationItem, ct);
            });

            When<Envelope<BuildingBecameUnderConstructionV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(message.Message.BuildingPersistentLocalId, message, item =>
                {
                    item.Status = MapBuildingStatus(BuildingRegistry.Building.BuildingStatus.UnderConstruction);
                }, ct);
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(message.Message.BuildingPersistentLocalId, message, item =>
                {
                    item.Status = MapBuildingStatus(BuildingRegistry.Building.BuildingStatus.Realized);
                }, ct);
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(message.Message.BuildingPersistentLocalId, message, item =>
                {
                    item.Status = MapBuildingStatus(BuildingRegistry.Building.BuildingStatus.UnderConstruction);
                }, ct);
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(message.Message.BuildingPersistentLocalId, message, item =>
                {
                    item.Status = MapBuildingStatus(BuildingRegistry.Building.BuildingStatus.NotRealized);
                }, ct);
            });

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    x =>
                    {
                        var buildingUnitSyndicationItem = new BuildingUnitSyndicationItemV2
                        {
                            Position = message.Position,
                            PersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                            Status = BuildingRegistry.Building.BuildingUnitStatus.Planned,
                            Function = BuildingRegistry.Building.BuildingUnitFunction.Parse(message.Message.Function),
                            PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray(),
                            PositionMethod = BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod),
                            Version = message.Message.Provenance.Timestamp,
                            Addresses = new Collection<BuildingUnitAddressSyndicationItemV2>()
                        };

                        x.BuildingUnitsV2.Add(buildingUnitSyndicationItem);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(message.Message.BuildingPersistentLocalId, message, item =>
                {
                    var unit = item.BuildingUnitsV2.Single(y => y.PersistentLocalId == message.Message.BuildingUnitPersistentLocalId);
                    unit.Status = BuildingRegistry.Building.BuildingUnitStatus.Realized;
                    unit.Version = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(message.Message.BuildingPersistentLocalId, message, item =>
                {
                    var unit = item.BuildingUnitsV2.Single(y => y.PersistentLocalId == message.Message.BuildingUnitPersistentLocalId);
                    unit.Status = BuildingRegistry.Building.BuildingUnitStatus.Planned;
                    unit.Version = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(message.Message.BuildingPersistentLocalId, message, item =>
                {
                    var unit = item.BuildingUnitsV2.Single(y => y.PersistentLocalId == message.Message.BuildingUnitPersistentLocalId);
                    unit.Status = BuildingRegistry.Building.BuildingUnitStatus.NotRealized;
                    unit.Version = message.Message.Provenance.Timestamp;
                }, ct);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingSyndicationItem(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    x =>
                    {
                        var commonBuildingUnitSyndicationItem = new BuildingUnitSyndicationItemV2
                        {
                            Position = message.Position,
                            PersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                            Status = BuildingRegistry.Building.BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus),
                            Function = BuildingRegistry.Building.BuildingUnitFunction.Common,
                            PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray(),
                            PositionMethod = BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod),
                            Version = message.Message.Provenance.Timestamp,
                            Addresses = new Collection<BuildingUnitAddressSyndicationItemV2>()
                        };

                        x.BuildingUnitsV2.Add(commonBuildingUnitSyndicationItem);
                    },
                    ct);
            });
        }

        private static BuildingGeometryMethod MapBuildingGeometryMethod(BuildingRegistry.Building.BuildingGeometryMethod buildingGeometryMethod)
        {
            var dictionary = new Dictionary<BuildingRegistry.Building.BuildingGeometryMethod, BuildingGeometryMethod>
            {
                { BuildingRegistry.Building.BuildingGeometryMethod.MeasuredByGrb, BuildingGeometryMethod.MeasuredByGrb },
                { BuildingRegistry.Building.BuildingGeometryMethod.Outlined, BuildingGeometryMethod.Outlined }
            };

            return dictionary[buildingGeometryMethod];
        }

        private static BuildingStatus MapBuildingStatus(BuildingRegistry.Building.BuildingStatus buildingStatus)
        {
            var dictionary = new Dictionary<BuildingRegistry.Building.BuildingStatus, BuildingStatus>
            {
                { BuildingRegistry.Building.BuildingStatus.Planned, BuildingStatus.Planned },
                { BuildingRegistry.Building.BuildingStatus.UnderConstruction, BuildingStatus.UnderConstruction },
                { BuildingRegistry.Building.BuildingStatus.NotRealized, BuildingStatus.NotRealized },
                { BuildingRegistry.Building.BuildingStatus.Realized, BuildingStatus.Realized },
                { BuildingRegistry.Building.BuildingStatus.Retired, BuildingStatus.Retired }
            };

            return dictionary[buildingStatus];
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
                {
                    buildingUnitDetailItem.Status = BuildingUnitStatus.NotRealized;
                }
                else if (buildingUnitIdsToRetire.Contains(buildingUnitDetailItem.BuildingUnitId))
                {
                    buildingUnitDetailItem.Status = BuildingUnitStatus.Retired;
                }

                buildingUnitDetailItem.Addresses.Clear();

                ApplyUnitVersion(buildingUnitDetailItem, version);
            }
        }

        private static Task DoNothing<T>(LegacyContext _, Envelope<T> __) where T : IMessage => Task.CompletedTask;
    }
}
