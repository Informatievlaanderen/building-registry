namespace BuildingRegistry.Consumer.Address.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.AddressRegistry;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Building;
    using Building.Commands;
    using Microsoft.EntityFrameworkCore;
    using NodaTime;
    using NodaTime.Text;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Provenance;

    public sealed class CommandHandlingKafkaProjection : ConnectedProjection<CommandHandler>
    {
        private readonly IDbContextFactory<BackOfficeContext> _backOfficeContextFactory;

        public CommandHandlingKafkaProjection(
            IDbContextFactory<BackOfficeContext> backOfficeContextFactory,
            IBuildings buildings)
        {
            _backOfficeContextFactory = backOfficeContextFactory;

            When<AddressWasMigratedToStreetName>(async (commandHandler, message, ct) =>
            {
                if (message.IsRemoved)
                {
                    await DetachBecauseRemoved(
                        commandHandler,
                        new AddressPersistentLocalId(message.AddressPersistentLocalId),
                        message.Provenance,
                        ct);
                }
                else if (message.Status == AddressStatus.Retired)
                {
                    await DetachBecauseRetired(
                        commandHandler,
                        new AddressPersistentLocalId(message.AddressPersistentLocalId),
                        message.Provenance,
                        ct);
                }
                else if (message.Status == AddressStatus.Rejected)
                {
                    await DetachBecauseRejected(
                        commandHandler,
                        new AddressPersistentLocalId(message.AddressPersistentLocalId),
                        message.Provenance,
                        ct);
                }
            });

            When<AddressWasRejected>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRejectedBecauseHouseNumberWasRejected>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRejectedBecauseHouseNumberWasRetired>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRejectedBecauseStreetNameWasRetired>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRejectedBecauseStreetNameWasRejected>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRetiredV2>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRetired(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRetiredBecauseHouseNumberWasRetired>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRetired(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRetiredBecauseStreetNameWasRejected>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRetired(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRetiredBecauseStreetNameWasRetired>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRetired(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRemovedBecauseStreetNameWasRemoved>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRemoved(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRemovedV2>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRemoved(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRemovedBecauseHouseNumberWasRemoved>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRemoved(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<StreetNameWasReaddressed>(async (commandHandler, message, ct) =>
            {
                await using var backOfficeContext = await _backOfficeContextFactory.CreateDbContextAsync(ct);

                var readdresses = message.ReaddressedHouseNumbers
                    .Select(x => new ReaddressData(
                        new AddressPersistentLocalId(x.ReaddressedHouseNumber.SourceAddressPersistentLocalId),
                        new AddressPersistentLocalId(x.ReaddressedHouseNumber.DestinationAddressPersistentLocalId)))
                    .Concat(
                        message.ReaddressedHouseNumbers
                            .SelectMany(x => x.ReaddressedBoxNumbers)
                            .Select(boxNumberAddress => new ReaddressData(
                                new AddressPersistentLocalId(boxNumberAddress.SourceAddressPersistentLocalId),
                                new AddressPersistentLocalId(boxNumberAddress.DestinationAddressPersistentLocalId))))
                    .ToList();

                var sourceAddressPersistentLocalIds = readdresses
                    .Select(x => (int)x.SourceAddressPersistentLocalId)
                    .ToList();

                var buildingUnitAddressRelations = await backOfficeContext.BuildingUnitAddressRelation
                    .AsNoTracking()
                    .Where(x => sourceAddressPersistentLocalIds.Contains(x.AddressPersistentLocalId))
                    .ToListAsync(cancellationToken: ct);

                var commandByBuildings = new Dictionary<BuildingPersistentLocalId, ReaddressAddresses>();
                foreach (var addressRelationsByBuilding in buildingUnitAddressRelations
                             .GroupBy(x => x.BuildingPersistentLocalId))
                {
                    var addressesByBuildingUnit = new Dictionary<BuildingUnitPersistentLocalId, List<ReaddressData>>();
                    foreach (var buildingUnitAddressRelation in addressRelationsByBuilding)
                    {
                        var readdressData = readdresses
                            .Where(x => x.SourceAddressPersistentLocalId == buildingUnitAddressRelation.AddressPersistentLocalId)
                            .ToList();

                        if (readdressData.Any())
                        {
                            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(buildingUnitAddressRelation.BuildingUnitPersistentLocalId);
                            addressesByBuildingUnit.TryAdd(buildingUnitPersistentLocalId, []);
                            addressesByBuildingUnit[buildingUnitPersistentLocalId].AddRange(readdressData);
                        }
                    }

                    var buildingPersistentLocalId = new BuildingPersistentLocalId(addressRelationsByBuilding.Key);
                    commandByBuildings.Add(
                        buildingPersistentLocalId,
                        new ReaddressAddresses(buildingPersistentLocalId, addressesByBuildingUnit
                            .ToDictionary(x => x.Key, x => (IReadOnlyList<ReaddressData>)x.Value),
                            FromProvenance(message.Provenance)));
                }

                foreach (var command in commandByBuildings.Values)
                {
                    try
                    {
                        await commandHandler.HandleIdempotent(command, ct);
                    }
                    catch (IdempotencyException)
                    {
                        // Do nothing
                    }
                }

                var commandBuildingPersistentLocalIds = commandByBuildings.Values.Select(x => (int)x.BuildingPersistentLocalId).ToList();
                var allBackOfficeBuildingUnitAddressRelations = (await backOfficeContext.BuildingUnitAddressRelation
                        .AsNoTracking()
                        .Where(x => commandBuildingPersistentLocalIds.Contains(x.BuildingPersistentLocalId))
                        .ToListAsync(cancellationToken: ct))
                    .ToList();

                foreach (var readdressAddressedCommand in commandByBuildings.Values)
                {
                    var buildingPersistentLocalId = readdressAddressedCommand.BuildingPersistentLocalId;

                    var building = await buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), ct);

                    foreach (var buildingUnitReaddress in readdressAddressedCommand.Readdresses)
                    {
                        var buildingUnitPersistentLocalId = buildingUnitReaddress.Key;
                        var buildingUnit = building.BuildingUnits.Single(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

                        var buildingUnitBackOfficeAddresses = allBackOfficeBuildingUnitAddressRelations
                            .Where(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId)
                            .Select(x => new AddressPersistentLocalId(x.AddressPersistentLocalId))
                            .ToList();
                        var addressesToRemove = buildingUnitBackOfficeAddresses.Except(buildingUnit.AddressPersistentLocalIds).ToList();
                        var addressesToAdd = buildingUnit.AddressPersistentLocalIds.Except(buildingUnitBackOfficeAddresses).ToList();

                        foreach (var addressPersistentLocalId in addressesToRemove)
                        {
                            await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(
                                buildingUnitPersistentLocalId, addressPersistentLocalId, ct, saveChanges: false);
                        }

                        foreach (var addressPersistentLocalId in addressesToAdd)
                        {
                            await backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                                buildingPersistentLocalId, buildingUnitPersistentLocalId, addressPersistentLocalId, ct, saveChanges: false);
                        }
                    }

                    await backOfficeContext.SaveChangesAsync(ct);
                }
            });

            When<AddressWasRejectedBecauseOfReaddress>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRejected(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });

            When<AddressWasRetiredBecauseOfReaddress>(async (commandHandler, message, ct) =>
            {
                await DetachBecauseRetired(
                    commandHandler,
                    new AddressPersistentLocalId(message.AddressPersistentLocalId),
                    message.Provenance,
                    ct);
            });
        }

        private async Task DetachBecauseRemoved(
            CommandHandler commandHandler,
            AddressPersistentLocalId addressPersistentLocalId,
            Contracts.Provenance provenance,
            CancellationToken ct)
        {
            await using var backOfficeContext = await _backOfficeContextFactory.CreateDbContextAsync(ct);
            var relations = backOfficeContext.BuildingUnitAddressRelation
                .AsNoTracking()
                .Where(x => x.AddressPersistentLocalId == new AddressPersistentLocalId(addressPersistentLocalId))
                .ToList();

            foreach (var relation in relations)
            {
                var command = new DetachAddressFromBuildingUnitBecauseAddressWasRemoved(
                    new BuildingPersistentLocalId(relation.BuildingPersistentLocalId),
                    new BuildingUnitPersistentLocalId(relation.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(relation.AddressPersistentLocalId),
                    FromProvenance(provenance));
                await commandHandler.Handle(command, ct);

                await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(command.BuildingUnitPersistentLocalId, command.AddressPersistentLocalId, ct);
            }
        }

        private async Task DetachBecauseRetired(
            CommandHandler commandHandler,
            AddressPersistentLocalId addressPersistentLocalId,
            Contracts.Provenance provenance,
            CancellationToken ct)
        {
            await using var backOfficeContext = await _backOfficeContextFactory.CreateDbContextAsync(ct);
            var relations = backOfficeContext.BuildingUnitAddressRelation
                .AsNoTracking()
                .Where(x => x.AddressPersistentLocalId == new AddressPersistentLocalId(addressPersistentLocalId))
                .ToList();

            foreach (var relation in relations)
            {
                var command = new DetachAddressFromBuildingUnitBecauseAddressWasRetired(
                    new BuildingPersistentLocalId(relation.BuildingPersistentLocalId),
                    new BuildingUnitPersistentLocalId(relation.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(relation.AddressPersistentLocalId),
                    FromProvenance(provenance));
                await commandHandler.Handle(command, ct);

                await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(command.BuildingUnitPersistentLocalId, command.AddressPersistentLocalId, ct);
            }
        }

        private async Task DetachBecauseRejected(
            CommandHandler commandHandler,
            AddressPersistentLocalId addressPersistentLocalId,
            Contracts.Provenance provenance,
            CancellationToken ct)
        {
            await using var backOfficeContext = await _backOfficeContextFactory.CreateDbContextAsync(ct);
            var relations = backOfficeContext.BuildingUnitAddressRelation
                .AsNoTracking()
                .Where(x => x.AddressPersistentLocalId == new AddressPersistentLocalId(addressPersistentLocalId))
                .ToList();

            foreach (var relation in relations)
            {
                var command = new DetachAddressFromBuildingUnitBecauseAddressWasRejected(
                    new BuildingPersistentLocalId(relation.BuildingPersistentLocalId),
                    new BuildingUnitPersistentLocalId(relation.BuildingUnitPersistentLocalId),
                    new AddressPersistentLocalId(relation.AddressPersistentLocalId),
                    FromProvenance(provenance));
                await commandHandler.Handle(command, ct);

                await backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(command.BuildingUnitPersistentLocalId, command.AddressPersistentLocalId, ct);
            }
        }

        private static Provenance FromProvenance(Contracts.Provenance provenance) =>
            new(
                InstantPattern.General.Parse(provenance.Timestamp).Value,
                Enum.Parse<Application>(Application.AddressRegistry.ToString()),
                new Reason(provenance.Reason),
                new Operator(string.Empty),
                Enum.Parse<Modification>(Modification.Update.ToString()),
                Enum.Parse<Organisation>(provenance.Organisation));
    }
}
