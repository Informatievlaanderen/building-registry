namespace BuildingRegistry.Projections.BackOffice
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using Building;
    using Building.Events;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;

    public class BackOfficeProjections : ConnectedProjection<BackOfficeProjectionsContext>
    {
        public BackOfficeProjections(IDbContextFactory<BackOfficeContext> backOfficeContextFactory, IConfiguration configuration)
        {
            var delayInSeconds = configuration.GetValue("DelayInSeconds", 10);

            When<Envelope<BuildingUnitWasPlannedV2>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentBuildingUnitBuilding(
                    new BuildingPersistentLocalId(message.Message.BuildingPersistentLocalId),
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId), cancellationToken);
                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentBuildingUnitBuilding(
                    new BuildingPersistentLocalId(message.Message.BuildingPersistentLocalId),
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId), cancellationToken);
                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                    new BuildingPersistentLocalId(message.Message.BuildingPersistentLocalId),
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);

                await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(message.Message.PreviousAddressPersistentLocalId),
                    cancellationToken);

                await backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                    new BuildingPersistentLocalId(message.Message.BuildingPersistentLocalId),
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(message.Message.NewAddressPersistentLocalId),
                    cancellationToken);
            });

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(async (_, message, cancellationToken) =>
            {
                await DelayProjection(message, delayInSeconds, cancellationToken);

                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);

                var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId);
                var destinationBuildingPersistentLocalId = new BuildingPersistentLocalId(message.Message.DestinationBuildingPersistentLocalId);

                var buildingBuildingUnit = await backOfficeContext
                    .FindBuildingUnitBuildingRelation(buildingUnitPersistentLocalId, cancellationToken);

                if (buildingBuildingUnit is not null
                    && buildingBuildingUnit!.BuildingPersistentLocalId == message.Message.BuildingPersistentLocalId)
                {
                    buildingBuildingUnit!.BuildingPersistentLocalId = destinationBuildingPersistentLocalId;
                }

                await backOfficeContext.MoveBuildingUnitAddressRelations(
                    buildingUnitPersistentLocalId,
                    destinationBuildingPersistentLocalId,
                    cancellationToken,
                    saveChanges: false);

                await backOfficeContext.SaveChangesAsync(cancellationToken);
            });
        }

        private static async Task DelayProjection<TMessage>(Envelope<TMessage> envelope, int delayInSeconds, CancellationToken cancellationToken)
            where TMessage : IMessage
        {
            var differenceInSeconds = (DateTime.UtcNow - envelope.CreatedUtc).TotalSeconds;
            if (differenceInSeconds < delayInSeconds)
            {
                await Task.Delay(TimeSpan.FromSeconds(delayInSeconds - differenceInSeconds), cancellationToken);
            }
        }
    }
}
