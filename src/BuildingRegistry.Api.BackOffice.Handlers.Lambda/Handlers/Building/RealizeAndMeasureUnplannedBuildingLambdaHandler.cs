namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
    using Abstractions;
    using Abstractions.Building;
    using Abstractions.Validation;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Datastructures;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Requests.Building;
    using TicketingService.Abstractions;

    public sealed class RealizeAndMeasureUnplannedBuildingLambdaHandler : BuildingLambdaHandler<RealizeAndMeasureUnplannedBuildingLambdaRequest>
    {
        private const string MainBuildingGrbObjectType = "1";

        private readonly IParcelMatching _parcelMatching;
        private readonly IAddresses _addresses;
        private readonly BackOfficeContext _backOfficeContext;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;
        private readonly ILifetimeScope _lifetimeScope;
        private readonly bool _toggleAutomaticBuildingUnitCreationEnabled;

        public RealizeAndMeasureUnplannedBuildingLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings,
            IParcelMatching parcelMatching,
            IAddresses addresses,
            BackOfficeContext backOfficeContext,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            ILifetimeScope lifetimeScope)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                buildings)
        {
            _parcelMatching = parcelMatching;
            _addresses = addresses;
            _backOfficeContext = backOfficeContext;
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
            _lifetimeScope = lifetimeScope;
            _toggleAutomaticBuildingUnitCreationEnabled = configuration.GetValue<bool>("AutomaticBuildingUnitCreationToggle", false);
        }

        protected override async Task<object> InnerHandle(RealizeAndMeasureUnplannedBuildingLambdaRequest request,
            CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            if(_toggleAutomaticBuildingUnitCreationEnabled)
                await RealizeUnplannedBuildingUnit(request, cancellationToken);

            var lastHash = await GetHash(new BuildingPersistentLocalId(request.BuildingPersistentLocalId), cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, request.BuildingPersistentLocalId), lastHash);
        }

        private async Task RealizeUnplannedBuildingUnit(RealizeAndMeasureUnplannedBuildingLambdaRequest request, CancellationToken cancellationToken)
        {
            if (request.Request.GrbData.GrbObjectType != MainBuildingGrbObjectType)
            {
                return;
            }

            var buildingGeometry = WKBReaderFactory.Create().Read(request.Request.GrbData.GeometriePolygoon.ToExtendedWkbGeometry());
            var overlappingParcels = await _parcelMatching.GetUnderlyingParcels(buildingGeometry);

            var addresses = overlappingParcels
                .SelectMany(x => x.Addresses)
                .ToList();

            var activeAddresses = addresses.Any()
                ? await GetActiveAddresses(addresses)
                : new List<AddressData>();

            if (activeAddresses.Count != 1)
            {
                return;
            }

            var addressPersistentLocalId = activeAddresses.First().AddressPersistentLocalId;

            var buildingUnitAddressRelations = await _backOfficeContext
                .BuildingUnitAddressRelation
                .Where(x => x.AddressPersistentLocalId == addressPersistentLocalId)
                .ToListAsync(cancellationToken: cancellationToken);

            if (buildingUnitAddressRelations.Any())
            {
                return;
            }

            var buildingUnitPersistentLocalId =
                new BuildingUnitPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());
            var buildingUnitCommand = new RealizeUnplannedBuildingUnit(
                new BuildingPersistentLocalId(request.BuildingPersistentLocalId),
                buildingUnitPersistentLocalId,
                addressPersistentLocalId,
                request.Provenance);

            try
            {
                await using var scope = _lifetimeScope.BeginLifetimeScope();
                await scope.Resolve<IIdempotentCommandHandler>().Dispatch(
                    buildingUnitCommand.CreateCommandId(),
                    buildingUnitCommand,
                    request.Metadata,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                var aggregate =
                    await Buildings.GetAsync(new BuildingStreamId(buildingUnitCommand.BuildingPersistentLocalId), cancellationToken);
                var buildingUnit = aggregate.BuildingUnits.Single();
                buildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId;
            }

            await _backOfficeContext.AddIdempotentBuildingUnitBuilding(
                buildingUnitCommand.BuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                cancellationToken);
            await _backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                buildingUnitCommand.BuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                buildingUnitCommand.AddressPersistentLocalId,
                cancellationToken);
        }

        private async Task<List<AddressData>> GetActiveAddresses(List<AddressPersistentLocalId> addressPersistentLocalIds)
        {
            var addresses = await _addresses.GetAddresses(addressPersistentLocalIds);

            var validStatuses = new[] { AddressStatus.Current, AddressStatus.Proposed };

            return addresses.Where(x => validStatuses.Contains(x.Status) && !x.IsRemoved).ToList();
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, RealizeAndMeasureUnplannedBuildingLambdaRequest request)
        {
            return exception switch
            {
                PolygonIsInvalidException => ValidationErrors.Common.InvalidBuildingPolygonGeometry.ToTicketError(),
                _ => null
            };
        }
    }
}
