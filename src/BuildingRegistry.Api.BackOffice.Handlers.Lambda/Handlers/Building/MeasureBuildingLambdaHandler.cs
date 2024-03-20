namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.Building;
    using TicketingService.Abstractions;

    public sealed class MeasureBuildingLambdaHandler : BuildingLambdaHandler<MeasureBuildingLambdaRequest>
    {
        public MeasureBuildingLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                buildings)
        { }

        protected override async Task<object> InnerHandle(MeasureBuildingLambdaRequest request, CancellationToken cancellationToken)
        {
            var command = request.ToCommand();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    command.CreateCommandId(),
                    command,
                    request.Metadata,
                    cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            var lastHash = await GetHash(command.BuildingPersistentLocalId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, command.BuildingPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, MeasureBuildingLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => ValidationErrors.MeasureBuilding.BuildingInvalidStatus.ToTicketError(),
                PolygonIsInvalidException => ValidationErrors.Common.InvalidBuildingPolygonGeometry.ToTicketError(),
                _ => null
            };
        }
    }
}
