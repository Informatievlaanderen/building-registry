namespace BuildingRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
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

        public ProducerBuildingProjections(IProducer producer, ISnapshotManager snapshotManager, string osloNamespace)
        {
            _producer = producer;

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

            // Building Units
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
            When<Store.Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (_, message, ct) =>
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

            When<Store.Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (_, message, ct) =>
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

            When<Store.Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (_, message, ct) =>
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

            When<Store.Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (_, message, ct) =>
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

            When<Store.Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (_, message, ct) =>
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

            When<Store.Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (_, message, ct) =>
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

            When<Store.Envelope<BuildingUnitWasNotRealizedV2>>(async (_, message, ct) =>
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

            When<Store.Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (_, message, ct) =>
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

            When<Store.Envelope<BuildingUnitWasRealizedV2>>(async (_, message, ct) =>
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

            When<Store.Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (_, message, ct) =>
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

            When<Store.Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (_, message, ct) =>
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

            When<Store.Envelope<BuildingUnitWasRetiredV2>>(async (_, message, ct) =>
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
    }
}
