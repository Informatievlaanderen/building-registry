namespace BuildingRegistry.Projections.Legacy.BuildingUnitDetailV2WithCount
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;

    [ConnectedProjectionName("API endpoint detail/lijst gebouweenheden")]
    [ConnectedProjectionDescription("Projectie die de gebouweenheden data voor het gebouweenheden detail & lijst voorziet.")]
    public class BuildingUnitDetailV2Projections : ConnectedProjection<LegacyContext>
    {
        public BuildingUnitDetailV2Projections()
        {
            #region Building

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    var addressesIdsGrouped = buildingUnit.AddressPersistentLocalIds.GroupBy(x => x);
                    var addresses = addressesIdsGrouped
                        .Select(groupedAddressId =>
                            new BuildingUnitDetailAddressItemV2(buildingUnit.BuildingUnitPersistentLocalId, groupedAddressId.Key))
                        .ToList();

                    var buildingUnitDetailItemV2 = new BuildingUnitDetailItemV2(
                        buildingUnit.BuildingUnitPersistentLocalId,
                        message.Message.BuildingPersistentLocalId,
                        buildingUnit.ExtendedWkbGeometry.ToByteArray(),
                        BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod),
                        BuildingUnitFunction.Parse(buildingUnit.Function),
                        BuildingUnitStatus.Parse(buildingUnit.Status),
                        false,
                        new Collection<BuildingUnitDetailAddressItemV2>(addresses),
                        buildingUnit.IsRemoved,
                        message.Message.Provenance.Timestamp);

                    UpdateHash(buildingUnitDetailItemV2, message);

                    await context.BuildingUnitDetailsV2WithCount.AddAsync(
                        buildingUnitDetailItemV2
                        , ct);
                }
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    await Update(context, buildingUnitPersistentLocalId, item =>
                    {
                        item.Position = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                        item.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                        item.Version = message.Message.Provenance.Timestamp;
                        UpdateHash(item, message);
                    }, ct);
                }
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message
                             .BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await Update(context, buildingUnitPersistentLocalId, item =>
                    {
                        item.Position = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                        item.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                        item.Version = message.Message.Provenance.Timestamp;
                        UpdateHash(item, message);
                    }, ct);
                }
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message
                             .BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await Update(context, buildingUnitPersistentLocalId, item =>
                    {
                        item.Position = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                        item.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                        item.Version = message.Message.Provenance.Timestamp;
                        UpdateHash(item, message);
                    }, ct);
                }
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in
                         message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await Update(context, buildingUnitPersistentLocalId, item =>
                    {
                        item.Position = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                        item.PositionMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
                        item.Version = message.Message.Provenance.Timestamp;
                        UpdateHash(item, message);
                    }, ct);
                }
            });

            #endregion

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var buildingUnitDetailItemV2 = new BuildingUnitDetailItemV2(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Message.BuildingPersistentLocalId,
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod),
                    BuildingUnitFunction.Parse(message.Message.Function),
                    BuildingUnitStatus.Planned,
                    message.Message.HasDeviation,
                    new Collection<BuildingUnitDetailAddressItemV2>(),
                    isRemoved: false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(buildingUnitDetailItemV2, message);

                await context.BuildingUnitDetailsV2WithCount.AddAsync(buildingUnitDetailItemV2, ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Realized;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Realized;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Planned;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Planned;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.NotRealized;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.NotRealized;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Planned;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Retired;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Realized;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.IsRemoved = true;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.IsRemoved = true;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus);
                    item.HasDeviation = message.Message.HasDeviation;
                    item.Function = BuildingUnitFunction.Parse(message.Message.Function);
                    item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
                    item.PositionMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod);
                    item.IsRemoved = false;
                    item.Addresses = new Collection<BuildingUnitDetailAddressItemV2>();
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.HasDeviation = false;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.HasDeviation = true;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.HasDeviation = true;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.HasDeviation = false;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var commonBuildingUnitDetailItemV2 = new BuildingUnitDetailItemV2(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Message.BuildingPersistentLocalId,
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod),
                    BuildingUnitFunction.Common,
                    BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus),
                    message.Message.HasDeviation,
                    new Collection<BuildingUnitDetailAddressItemV2>(),
                    isRemoved: false,
                    message.Message.Provenance.Timestamp);

                UpdateHash(commonBuildingUnitDetailItemV2, message);

                await context.BuildingUnitDetailsV2WithCount.AddAsync(
                    commonBuildingUnitDetailItemV2
                    , ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
                    item.PositionMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod);
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    context.Entry(item).Collection(x => x.Addresses).Load();

                    item.Addresses.Add(new BuildingUnitDetailAddressItemV2(message.Message.BuildingUnitPersistentLocalId,
                        message.Message.AddressPersistentLocalId));
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    context.Entry(item).Collection(x => x.Addresses).Load();

                    var itemToRemove = item.Addresses.Single(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                    item.Addresses.Remove(itemToRemove);
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    context.Entry(item).Collection(x => x.Addresses).Load();

                    var itemToRemove = item.Addresses.Single(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                    item.Addresses.Remove(itemToRemove);
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    context.Entry(item).Collection(x => x.Addresses).Load();

                    var itemToRemove = item.Addresses.Single(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                    item.Addresses.Remove(itemToRemove);
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    context.Entry(item).Collection(x => x.Addresses).Load();

                    var itemToRemove = item.Addresses.Single(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                    item.Addresses.Remove(itemToRemove);
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    context.Entry(item).Collection(x => x.Addresses).Load();

                    var previousAddress = item.Addresses.SingleOrDefault(parcelAddress =>
                        parcelAddress.AddressPersistentLocalId == message.Message.PreviousAddressPersistentLocalId);

                    if (previousAddress is not null && previousAddress.Count == 1)
                    {
                        item.Addresses.Remove(previousAddress);
                    }
                    else if (previousAddress is not null)
                    {
                        previousAddress.Count -= 1;
                    }

                    var newAddress = item.Addresses.SingleOrDefault(parcelAddress =>
                        parcelAddress.AddressPersistentLocalId == message.Message.NewAddressPersistentLocalId);

                    if (newAddress is null)
                    {
                        item.Addresses.Add(new BuildingUnitDetailAddressItemV2(
                            message.Message.BuildingUnitPersistentLocalId,
                            message.Message.NewAddressPersistentLocalId));
                    }
                    else
                    {
                        newAddress.Count += 1;
                    }

                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitReaddresses in message.Message.BuildingUnitsReaddresses)
                {
                    await Update(context, buildingUnitReaddresses.BuildingUnitPersistentLocalId, item =>
                    {
                        context.Entry(item).Collection(x => x.Addresses).Load();

                        foreach (var addressPersistentLocalId in buildingUnitReaddresses.DetachedAddressPersistentLocalIds)
                        {
                            RemoveIdempotentAddress(item, addressPersistentLocalId);
                        }

                        foreach (var addressPersistentLocalId in buildingUnitReaddresses.AttachedAddressPersistentLocalIds)
                        {
                            AddIdempotentAddress(item,
                                new BuildingUnitDetailAddressItemV2(
                                    buildingUnitReaddresses.BuildingUnitPersistentLocalId,
                                    addressPersistentLocalId));
                        }

                        item.Version = message.Message.Provenance.Timestamp;
                        UpdateHash(item, message);
                    }, ct);
                }
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    context.Entry(item).Collection(x => x.Addresses).Load();

                    var previousAddress = item.Addresses.SingleOrDefault(parcelAddress =>
                        parcelAddress.AddressPersistentLocalId == message.Message.PreviousAddressPersistentLocalId);

                    if (previousAddress is not null)
                    {
                        item.Addresses.Remove(previousAddress);
                    }

                    var newAddress = item.Addresses.SingleOrDefault(parcelAddress =>
                        parcelAddress.AddressPersistentLocalId == message.Message.NewAddressPersistentLocalId);

                    if (newAddress is null)
                    {
                        item.Addresses.Add(new BuildingUnitDetailAddressItemV2(
                            message.Message.BuildingUnitPersistentLocalId,
                            message.Message.NewAddressPersistentLocalId));
                    }

                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.Retired;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.Status = BuildingUnitStatus.NotRealized;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                await Update(context, message.Message.BuildingUnitPersistentLocalId, item =>
                {
                    item.BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId;

                    item.Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus);
                    item.HasDeviation = message.Message.HasDeviation;
                    item.Function = BuildingUnitFunction.Parse(message.Message.Function);
                    item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
                    item.PositionMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod);
                    item.IsRemoved = false;
                    item.Version = message.Message.Provenance.Timestamp;
                    UpdateHash(item, message);
                }, ct);
            });
        }

        private static void RemoveIdempotentAddress(BuildingUnitDetailItemV2 buildingUnit, int addressPersistentLocalId)
        {
            var address = buildingUnit.Addresses.FirstOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);

            if (address is not null)
            {
                buildingUnit.Addresses.Remove(address);
            }
        }

        private static void AddIdempotentAddress(BuildingUnitDetailItemV2 buildingUnit,
            BuildingUnitDetailAddressItemV2 buildingUnitDetailAddressItemV2)
        {
            var address = buildingUnit.Addresses.FirstOrDefault(x =>
                x.AddressPersistentLocalId == buildingUnitDetailAddressItemV2.AddressPersistentLocalId);

            if (address is null)
            {
                buildingUnit.Addresses.Add(buildingUnitDetailAddressItemV2);
            }
        }

        private static async Task Update(LegacyContext context, int buildingUnitPersistentLocalId, Action<BuildingUnitDetailItemV2> updateAction,
            CancellationToken ct)
        {
            var item = await context
                .BuildingUnitDetailsV2WithCount
                .FindAsync(buildingUnitPersistentLocalId, cancellationToken: ct);

            updateAction(item!);
        }

        private static void UpdateHash<T>(BuildingUnitDetailItemV2 entity, Envelope<T> wrappedEvent) where T : IHaveHash, IMessage
        {
            if (!wrappedEvent.Metadata.ContainsKey(AddEventHashPipe.HashMetadataKey))
            {
                throw new InvalidOperationException($"Cannot find hash in metadata for event at position {wrappedEvent.Position}");
            }

            entity.LastEventHash = wrappedEvent.Metadata[AddEventHashPipe.HashMetadataKey].ToString()!;
        }
    }
}
