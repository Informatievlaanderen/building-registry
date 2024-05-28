namespace BuildingRegistry.Producer.Snapshot.Oslo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AllStream.Events;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Building.Events;

    [ConnectedProjectionName("Kafka producer snapshot oslo gebouweenheden")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerBuildingUnitProjections : ConnectedProjection<ProducerContext>
    {
        public const string TopicKey = "BuildingUnitTopic";

        private readonly IProducer _producer;

        public ProducerBuildingUnitProjections(
            IProducer producer,
            ISnapshotManager snapshotManager,
            string osloNamespace,
            IOsloProxy osloProxy)
        {
            _producer = producer;

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitOsloSnapshotsWereRequested>>(async (_, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    await FindAndProduce(async () =>
                            await osloProxy.GetSnapshot(buildingUnitPersistentLocalId.ToString(), ct),
                        message.Position,
                        ct);
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasMigrated>>(async (_, message, ct) =>
            {
                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    if (!buildingUnit.IsRemoved)
                    {
                        await FindAndProduce(async () =>
                                await snapshotManager.FindMatchingSnapshot(
                                    buildingUnit.BuildingUnitPersistentLocalId.ToString(),
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
                        await Produce($"{osloNamespace}/{buildingUnit.BuildingUnitPersistentLocalId}", buildingUnit.BuildingUnitPersistentLocalId.ToString(), "{}", message.Position, ct);
                    }
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingOutlineWasChanged>>(async (_, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                buildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasMeasured>>(async (_, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message
                             .BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                buildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingMeasurementWasCorrected>>(async (_, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in
                         message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                buildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitAddressWasAttachedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingUnitPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitAddressWasDetachedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingUnitPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitPositionWasCorrected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingUnitPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitRemovalWasCorrected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingUnitPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<
                BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingUnitPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasDeregulated>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingUnitPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitDeregulationWasCorrected>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasNotRealizedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingUnitPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasPlannedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingUnitPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRealizedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingUnitPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRegularized>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingUnitPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitRegularizationWasCorrected>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRetiredV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingUnitPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (_,
                    message, ct) =>
                { });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(
                async (_, message, ct) => { });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(
                async (_, message, ct) => { });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasNotRealizedV2>>(async (_, message, ct) => { });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasPlannedV2>>(async (_, message, ct) => { });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (_, message,
                    ct) =>
                { });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasRealizedV2>>(async (_, message, ct) => { });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingWasRemovedV2>>(async (_, message, ct) => { });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<CommonBuildingUnitWasAddedV2>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                        await snapshotManager.FindMatchingSnapshot(
                            message.Message.BuildingUnitPersistentLocalId.ToString(),
                            message.Message.Provenance.Timestamp,
                            message.Message.GetHash(),
                            message.Position,
                            throwStaleWhenGone: false,
                            ct),
                    message.Position,
                    ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRemovedV2>>(async (_, message, ct) =>
            {
                await Produce($"{osloNamespace}/{message.Message.BuildingUnitPersistentLocalId}", message.Message.BuildingUnitPersistentLocalId.ToString(), "{}", message.Position, ct);
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(
                async (_, message, ct) =>
                {
                    await Produce($"{osloNamespace}/{message.Message.BuildingUnitPersistentLocalId}", message.Message.BuildingUnitPersistentLocalId.ToString(), "{}", message.Position, ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(
                async (_, message, ct) =>
                {
                    foreach (var buildingUnitReaddresses in message.Message.BuildingUnitsReaddresses)
                    {
                        await FindAndProduce(async () =>
                                await snapshotManager.FindMatchingSnapshot(
                                    buildingUnitReaddresses.BuildingUnitPersistentLocalId.ToString(),
                                    message.Message.Provenance.Timestamp,
                                    message.Message.GetHash(),
                                    message.Position,
                                    throwStaleWhenGone: false,
                                    ct),
                            message.Position,
                            ct);
                    }
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(
                async (_, message, ct) =>
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                message.Message.BuildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingMeasurementWasChanged>>(async (_, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in
                         message.Message.BuildingUnitPersistentLocalIds.Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await FindAndProduce(async () =>
                            await snapshotManager.FindMatchingSnapshot(
                                buildingUnitPersistentLocalId.ToString(),
                                message.Message.Provenance.Timestamp,
                                message.Message.GetHash(),
                                message.Position,
                                throwStaleWhenGone: false,
                                ct),
                        message.Position,
                        ct);
                }
            });

            When<Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope<BuildingUnitWasMovedIntoBuilding>>(async (_, message, ct) =>
            {
                await FindAndProduce(async () =>
                    await snapshotManager.FindMatchingSnapshot(
                        message.Message.BuildingUnitPersistentLocalId.ToString(),
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
