namespace BuildingRegistry.Producer.Ldes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building.Events;

    [ConnectedProjectionName("Kafka producer ldes gebouweenheden")]
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

            When<Envelope<BuildingUnitOsloSnapshotsWereRequested>>(async (_, message, ct) =>
            {
                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        await FindAndProduce(async () =>
                                await osloProxy.GetSnapshot(buildingUnitPersistentLocalId.ToString(), ct),
                            message.Position,
                            ct);
                    }
                    catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Gone)
                    { }
                }
            });

            #region Building

            When<Envelope<BuildingWasMigrated>>(async (_, message, ct) =>
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

            When<Envelope<BuildingOutlineWasChanged>>(async (_, message, ct) =>
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

            When<Envelope<BuildingWasMeasured>>(async (_, message, ct) =>
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

            When<Envelope<BuildingMeasurementWasCorrected>>(async (_, message, ct) =>
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

            When<Envelope<BuildingWasPlannedV2>>(DoNothing);
            When<Envelope<BuildingBecameUnderConstructionV2>>(DoNothing);
            When<Envelope<BuildingWasRealizedV2>>(DoNothing);
            When<Envelope<BuildingWasNotRealizedV2>>(DoNothing);
            When<Envelope<BuildingWasDemolished>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(DoNothing);
            When<Envelope<BuildingGeometryWasImportedFromGrb>>(DoNothing);
            When<Envelope<BuildingWasRemovedV2>>(DoNothing);
            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(DoNothing);

            #endregion Building

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (_, message, ct) =>
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

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (_, message, ct) =>
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

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(
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

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(
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

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(
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

            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(
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

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (_, message, ct) =>
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

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (_, message, ct) =>
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

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(
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

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(
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

            When<Envelope<
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

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(
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

            When<Envelope<BuildingUnitWasDeregulated>>(async (_, message, ct) =>
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

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(
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

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(
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

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (_, message, ct) =>
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

            When<Envelope<BuildingUnitWasPlannedV2>>(async (_, message, ct) =>
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

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(
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

            When<Envelope<BuildingUnitWasRealizedV2>>(async (_, message, ct) =>
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

            When<Envelope<BuildingUnitWasRegularized>>(async (_, message, ct) =>
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

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(
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

            When<Envelope<BuildingUnitWasRetiredV2>>(async (_, message, ct) =>
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

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (_,
                    message, ct) =>
                { });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(
                async (_, message, ct) => { });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(
                async (_, message, ct) => { });

            When<Envelope<BuildingWasNotRealizedV2>>(async (_, message, ct) => { });

            When<Envelope<BuildingWasPlannedV2>>(async (_, message, ct) => { });

            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (_, message,
                    ct) =>
                { });

            When<Envelope<BuildingWasRealizedV2>>(async (_, message, ct) => { });

            When<Envelope<BuildingWasRemovedV2>>(async (_, message, ct) => { });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (_, message, ct) =>
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

            When<Envelope<BuildingUnitWasRemovedV2>>(async (_, message, ct) =>
            {
                await Produce($"{osloNamespace}/{message.Message.BuildingUnitPersistentLocalId}", message.Message.BuildingUnitPersistentLocalId.ToString(), "{}", message.Position, ct);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(
                async (_, message, ct) =>
                {
                    await Produce($"{osloNamespace}/{message.Message.BuildingUnitPersistentLocalId}", message.Message.BuildingUnitPersistentLocalId.ToString(), "{}", message.Position, ct);
                });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(
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

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(
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

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(
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

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(
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

            When<Envelope<BuildingMeasurementWasChanged>>(async (_, message, ct) =>
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

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (_, message, ct) =>
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

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(DoNothing);
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

        private static Task DoNothing<T>(ProducerContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
