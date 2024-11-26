namespace BuildingRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using AllStream.Events;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Building.Events;
    using Store = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;

    [ConnectedProjectionName("Kafka producer snapshot oslo gebouwen")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerBuildingProjections : ConnectedProjection<ProducerContext>
    {
        public const string TopicKey = "BuildingTopic";

        private readonly IProducer _producer;

        public ProducerBuildingProjections(
            IProducer producer,
            ISnapshotManager snapshotManager,
            string osloNamespace,
            IOsloProxy osloProxy)
        {
            _producer = producer;

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingOsloSnapshotsWereRequested>>(async (_, message, ct) =>
            {
                foreach (var buildingPersistentLocalId in message.Message.BuildingPersistentLocalIds)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        await FindAndProduce(async () =>
                                await osloProxy.GetSnapshot(buildingPersistentLocalId.ToString(), ct),
                            message.Position,
                            ct);
                    }
                    catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Gone)
                    { }
                }
            });

            When<Store.Envelope<BuildingWasMigrated>>(async (_, message, ct) =>
            {
                if (!message.Message.IsRemoved)
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                }
                else
                {
                    await Produce($"{osloNamespace}/{message.Message.BuildingPersistentLocalId}", message.Message.BuildingPersistentLocalId.ToString(), "{}", message.Position, ct);
                }
            });

            When<Store.Envelope<BuildingBecameUnderConstructionV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
            });

            When<Store.Envelope<BuildingOutlineWasChanged>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingWasNotRealizedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingWasPlannedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingWasRealizedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingWasRemovedV2>>(async (_, message, ct) =>
            {
                await Produce($"{osloNamespace}/{message.Message.BuildingPersistentLocalId}", message.Message.BuildingPersistentLocalId.ToString(), "{}", message.Position, ct);
            });

            When<Store.Envelope<BuildingWasMeasured>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingMeasurementWasCorrected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingWasDemolished>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingMeasurementWasChanged>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingGeometryWasImportedFromGrb>>(DoNothing);

            #region BuildingUnits

            When<Store.Envelope<CommonBuildingUnitWasAddedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingUnitWasPlannedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingUnitWasRemovedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingUnitRemovalWasCorrected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingUnitWasMovedIntoBuilding>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Store.Envelope<BuildingUnitWasMovedOutOfBuilding>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            //Normally we should update the snapshot for status changes of buildingunits because they are shown in the api call
            //but then we conflict with the change (timestamp) of the building and the projection will get stuck
            //to fix if we continue with this projection

            When<Store.Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasRegularized>>(DoNothing);
            When<Store.Envelope<BuildingUnitRegularizationWasCorrected>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasDeregulated>>(DoNothing);
            When<Store.Envelope<BuildingUnitDeregulationWasCorrected>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasRetiredV2>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(DoNothing);
            When<Store.Envelope<BuildingUnitPositionWasCorrected>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasRealizedV2>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasNotRealizedV2>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(DoNothing);
            When<Store.Envelope<BuildingUnitAddressWasAttachedV2>>(DoNothing);
            When<Store.Envelope<BuildingUnitAddressWasDetachedV2>>(DoNothing);
            When<Store.Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(DoNothing);
            When<Store.Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(DoNothing);
            When<Store.Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(DoNothing);
            When<Store.Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(DoNothing);
            When<Store.Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(DoNothing);
            When<Store.Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(DoNothing);

            #endregion
        }

        private async Task FindAndProduce(Func<Task<OsloResult?>> findMatchingSnapshot, long storePosition, CancellationToken ct)
        {
            var result = await findMatchingSnapshot.Invoke();

            if (result != null)
            {
                await Produce(result.Identificator.Id, result.Identificator.ObjectId, result.JsonContent, storePosition, ct);
            }
        }

        private async Task Produce(string puri, string objectId, string jsonContent, long storePosition, CancellationToken cancellationToken = default)
        {
            var result = await _producer.Produce(
                new MessageKey(puri),
                jsonContent,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, $"{objectId}-{storePosition.ToString()}") },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }

        private static Task DoNothing<T>(ProducerContext context, Store.Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
