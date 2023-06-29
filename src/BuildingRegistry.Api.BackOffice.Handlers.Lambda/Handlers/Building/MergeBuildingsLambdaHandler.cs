namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Validation;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.Building;
    using TicketingService.Abstractions;
    using BuildingUnit = BuildingRegistry.Building.BuildingUnit;

    public sealed class MergeBuildingsLambdaHandler : BuildingLambdaHandler<MergeBuildingsLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly ILifetimeScope _lifetimeScope;

        public MergeBuildingsLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings,
            BackOfficeContext backOfficeContext,
            ILifetimeScope lifetimeScope)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                buildings)
        {
            _backOfficeContext = backOfficeContext;
            _lifetimeScope = lifetimeScope;
        }

        protected override async Task<ETagResponse> InnerHandle(MergeBuildingsLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            // Transaction because a commonBuildingUnit is sometimes added
            await using var transaction = await _backOfficeContext.Database.BeginTransactionAsync(cancellationToken);

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

            await transaction.CommitAsync(cancellationToken);

            foreach (var mergedBuildingPersistentLocalId in cmd.BuildingPersistentLocalIdsToMerge)
            {
                await MarkBuildingAsMerged(
                    request,
                    mergedBuildingPersistentLocalId,
                    cmd.NewBuildingPersistentLocalId,
                    cancellationToken);
            }

            var newBuilding = await Buildings.GetAsync(new BuildingStreamId(cmd.NewBuildingPersistentLocalId), cancellationToken);

            foreach (var movedBuildingUnit in newBuilding.BuildingUnits)
            {
                await RecoupleBuildingUnitBuildingRelations(movedBuildingUnit, cmd.NewBuildingPersistentLocalId, cancellationToken);
                await RecoupleBuildingUnitAddressRelations(movedBuildingUnit, cmd.NewBuildingPersistentLocalId, cancellationToken);
            }

            var lastHash = await GetHash(request.BuildingPersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, request.BuildingPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, MergeBuildingsLambdaRequest request)
        {
            return exception switch
            {
                BuildingToMergeHasInvalidStatusException => ValidationErrors.MergeBuildings.BuildingInvalidStatus.ToTicketError(),
                BuildingMergerNeedsMoreThanOneBuildingException => ValidationErrors.MergeBuildings.TooFewBuildings.ToTicketError(),
                BuildingMergerHasTooManyBuildingsException => ValidationErrors.MergeBuildings.TooManyBuildings.ToTicketError(),
                BuildingToMergeHasInvalidGeometryMethodException => ValidationErrors.Common.InvalidBuildingPolygonGeometry.ToTicketError(),
                _ => null
            };
        }

        private async Task MarkBuildingAsMerged(
            MergeBuildingsLambdaRequest request,
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingPersistentLocalId newBuildingPersistentLocalId,
            CancellationToken cancellationToken)
        {
            try
            {
                await using var scope = _lifetimeScope.BeginLifetimeScope();

                var markCommand = new MarkBuildingAsMerged(
                    buildingPersistentLocalId,
                    newBuildingPersistentLocalId,
                    request.Provenance);

                await scope
                    .Resolve<IIdempotentCommandHandler>()
                    .Dispatch(
                        markCommand.CreateCommandId(),
                        markCommand,
                        request.Metadata,
                        cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }
        }

        private async Task RecoupleBuildingUnitAddressRelations(
            BuildingUnit movedBuildingUnit,
            BuildingPersistentLocalId newBuildingPersistentLocalId,
            CancellationToken cancellationToken)
        {
            await _backOfficeContext.RemoveBuildingUnitAddressRelations(
                movedBuildingUnit.BuildingUnitPersistentLocalId,
                cancellationToken);

            foreach (var addressPersistentLocalId in movedBuildingUnit.AddressPersistentLocalIds)
            {
                await _backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                    newBuildingPersistentLocalId,
                    movedBuildingUnit.BuildingUnitPersistentLocalId,
                    addressPersistentLocalId,
                    cancellationToken);
            }
        }

        private async Task RecoupleBuildingUnitBuildingRelations(
            BuildingUnit movedBuildingUnit,
            BuildingPersistentLocalId newBuildingPersistentLocalId,
            CancellationToken cancellationToken)
        {
            await _backOfficeContext.RemoveIdempotentBuildingUnitBuildingRelation(
                movedBuildingUnit.BuildingUnitPersistentLocalId,
                cancellationToken);
            await _backOfficeContext.AddIdempotentBuildingUnitBuilding(
                newBuildingPersistentLocalId,
                movedBuildingUnit.BuildingUnitPersistentLocalId,
                cancellationToken);
        }
    }
}
