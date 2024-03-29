﻿namespace BuildingRegistry.Projections.BackOffice
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using Building;
    using Building.Events;
    using Microsoft.EntityFrameworkCore;

    public class BackOfficeProjections : ConnectedProjection<BackOfficeProjectionsContext>
    {
        public BackOfficeProjections(IDbContextFactory<BackOfficeContext> backOfficeContextFactory)
        {
            When<Envelope<BuildingUnitWasPlannedV2>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentBuildingUnitBuilding(
                    message.Message.BuildingPersistentLocalId, message.Message.BuildingUnitPersistentLocalId, cancellationToken);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentBuildingUnitBuilding(
                    message.Message.BuildingPersistentLocalId, message.Message.BuildingUnitPersistentLocalId, cancellationToken);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                    new BuildingPersistentLocalId(message.Message.BuildingPersistentLocalId),
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (_, message, cancellationToken) =>
            {
                await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
                await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(
                    new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(message.Message.AddressPersistentLocalId),
                    cancellationToken);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (_, message, cancellationToken) =>
            {
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

            // When<Envelope<BuildingUnitWasTransferred>>(async (_, message, cancellationToken) =>
            // {
            //     await using var backOfficeContext = await backOfficeContextFactory.CreateDbContextAsync(cancellationToken);
            //
            //     var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(message.Message.BuildingUnitPersistentLocalId);
            //     var buildingPersistentLocalId = new BuildingPersistentLocalId(message.Message.BuildingPersistentLocalId);
            //
            //     await backOfficeContext.RemoveIdempotentBuildingUnitBuildingRelation(
            //         buildingUnitPersistentLocalId,
            //         cancellationToken);
            //
            //     await backOfficeContext.AddIdempotentBuildingUnitBuilding(
            //         buildingPersistentLocalId,
            //         buildingUnitPersistentLocalId,
            //         cancellationToken);
            //
            //     await backOfficeContext.RemoveBuildingUnitAddressRelations(
            //         buildingUnitPersistentLocalId,
            //         cancellationToken);
            //
            //     foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
            //     {
            //         await backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
            //             buildingPersistentLocalId,
            //             buildingUnitPersistentLocalId,
            //             new AddressPersistentLocalId(addressPersistentLocalId),
            //             cancellationToken);
            //     }
            // });
        }
    }
}
