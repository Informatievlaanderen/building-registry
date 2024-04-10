namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit
{
    using Abstractions;
    using Abstractions.Validation;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.BuildingUnit;
    using TicketingService.Abstractions;

    public sealed class MoveBuildingUnitLambdaHandler : BuildingUnitLambdaHandler<MoveBuildingUnitLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly ILifetimeScope _lifetimeScope;

        public MoveBuildingUnitLambdaHandler(
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
        
        protected override async Task<object> InnerHandle(MoveBuildingUnitLambdaRequest request, CancellationToken cancellationToken)
        {
            var moveIntoBuildingCommand = request.ToMoveBuildingUnitIntoBuildingCommand();
            var moveOutOfBuildingCommand = request.ToMoveBuildingUnitOutOfBuildingCommand();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    moveIntoBuildingCommand.CreateCommandId(),
                    moveIntoBuildingCommand,
                    request.Metadata,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            try
            {
                await using var scope = _lifetimeScope.BeginLifetimeScope();
                
                await scope.Resolve<IIdempotentCommandHandler>().Dispatch(
                    moveOutOfBuildingCommand.CreateCommandId(),
                    moveOutOfBuildingCommand,
                    request.Metadata,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            await using (var transaction = await _backOfficeContext.Database.BeginTransactionAsync(cancellationToken))
            {
                await _backOfficeContext.RemoveIdempotentBuildingUnitBuildingRelation(moveIntoBuildingCommand.BuildingUnitPersistentLocalId, cancellationToken);
                await _backOfficeContext.AddIdempotentBuildingUnitBuilding(moveIntoBuildingCommand.DestinationBuildingPersistentLocalId, moveIntoBuildingCommand.BuildingUnitPersistentLocalId, cancellationToken);
                await _backOfficeContext.MoveBuildingUnitAddressRelations(moveIntoBuildingCommand.BuildingUnitPersistentLocalId, moveIntoBuildingCommand.DestinationBuildingPersistentLocalId, cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            
            var lastHash = await GetHash(
                moveIntoBuildingCommand.DestinationBuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(request.BuildingUnitPersistentLocalId),
                cancellationToken);

            return new ETagResponse(string.Format(DetailUrlFormat, request.BuildingUnitPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, MoveBuildingUnitLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => ValidationErrors.MoveBuildingUnit.BuildingInvalidStatus.ToTicketError(),
                BuildingUnitHasInvalidFunctionException => ValidationErrors.Common.BuildingUnitHasInvalidFunction.ToTicketError(),
                _ => null
            };
        }
    }
}
