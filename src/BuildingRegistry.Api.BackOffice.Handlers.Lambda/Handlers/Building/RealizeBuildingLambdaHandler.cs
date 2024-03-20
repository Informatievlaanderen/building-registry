namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
    using Abstractions.Building.SqsRequests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.Building;
    using TicketingService.Abstractions;

    public sealed class RealizeBuildingLambdaHandler : BuildingLambdaHandler<RealizeBuildingLambdaRequest>
    {
        private readonly ISqsQueue _sqsQueue;
        private readonly bool _toggleAnoApiEnabled;

        public RealizeBuildingLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings,
            ISqsQueue sqsQueue)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                buildings)
        {
            _sqsQueue = sqsQueue;
            _toggleAnoApiEnabled = configuration.GetValue<bool>("AnoApiToggle", false);
        }

        protected override async Task<ETagResponse> InnerHandle(RealizeBuildingLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            try
            {
                var streamPositionIncrements = await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata,
                    cancellationToken);

                if (streamPositionIncrements > 0 && _toggleAnoApiEnabled)
                {
                    var building =
                        await Buildings.GetAsync(new BuildingStreamId(cmd.BuildingPersistentLocalId), cancellationToken);

                    var sqsRequest = new NotifyOutlinedRealizedBuildingSqsRequest(
                        cmd.BuildingPersistentLocalId,
                        request.Provenance.Organisation.ToString(),
                        DateTimeOffset.UtcNow,
                        building.BuildingGeometry.Geometry.ToString());
                    await _sqsQueue.Copy(sqsRequest, new SqsQueueOptions(), cancellationToken);
                }
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            var lastHash = await GetHash(new BuildingPersistentLocalId(request.BuildingPersistentLocalId), cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, request.BuildingPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, RealizeBuildingLambdaRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => ValidationErrors.RealizeBuilding.BuildingInvalidStatus.ToTicketError(),
                _ => null
            };
        }
    }
}
