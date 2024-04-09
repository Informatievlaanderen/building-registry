namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit
{
    using Abstractions;
    using Abstractions.Validation;
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

        public MoveBuildingUnitLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings,
            BackOfficeContext backOfficeContext)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                buildings)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override async Task<object> InnerHandle(MoveBuildingUnitLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToMoveBuildingUnitIntoBuildingCommand();

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

            await using (var transaction = await _backOfficeContext.Database.BeginTransactionAsync(cancellationToken))
            {
                await _backOfficeContext.RemoveIdempotentBuildingUnitBuildingRelation(cmd.BuildingUnitPersistentLocalId, cancellationToken);
                await _backOfficeContext.AddIdempotentBuildingUnitBuilding(cmd.DestinationBuildingPersistentLocalId, cmd.BuildingUnitPersistentLocalId, cancellationToken);
                await _backOfficeContext.MoveBuildingUnitAddressRelations(cmd.BuildingUnitPersistentLocalId, cmd.DestinationBuildingPersistentLocalId, CancellationToken.None);

                await transaction.CommitAsync(cancellationToken);
            }
            
            var lastHash = await GetHash(
                cmd.DestinationBuildingPersistentLocalId,
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
                BuildingUnitHasInvalidStatusException => ValidationErrors.MoveBuildingUnit.BuildingUnitInvalidStatus.ToTicketError(),
                _ => null
            };
        }
    }
}
