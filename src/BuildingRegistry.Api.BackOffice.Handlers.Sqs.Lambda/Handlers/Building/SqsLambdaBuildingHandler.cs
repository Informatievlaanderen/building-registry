namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers.Building
{
    using Abstractions.Building.Responses;
    using Abstractions.Building.Validators;
    using Abstractions.Exceptions;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using BuildingRegistry.Building;
    using BuildingRegistry.Infrastructure;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Requests.Building;
    using TicketingService.Abstractions;

    public abstract class SqsLambdaBuildingHandler<TSqsLambdaRequest> : IRequestHandler<TSqsLambdaRequest>
        where TSqsLambdaRequest : SqsLambdaBuildingRequest
    {
        private readonly ITicketing _ticketing;
        private readonly ICustomRetryPolicy _retryPolicy;
        private readonly IBuildings _buildings;

        protected IIdempotentCommandHandler IdempotentCommandHandler { get; }
        protected string DetailUrlFormat { get; }

        protected SqsLambdaBuildingHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings)
        {
            _retryPolicy = retryPolicy;
            _ticketing = ticketing;
            IdempotentCommandHandler = idempotentCommandHandler;
            _buildings = buildings;

            DetailUrlFormat = configuration["DetailUrl"];
            if (string.IsNullOrEmpty(DetailUrlFormat))
            {
                throw new NullReferenceException("'DetailUrl' cannot be found in the configuration");
            }
        }

        protected abstract Task<ETagResponse> InnerHandle(TSqsLambdaRequest request, CancellationToken cancellationToken);

        protected abstract TicketError? MapDomainException(DomainException exception, TSqsLambdaRequest request);

        public async Task<Unit> Handle(TSqsLambdaRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await ValidateIfMatchHeaderValue(request, cancellationToken);

                await _ticketing.Pending(request.TicketId, cancellationToken);

                ETagResponse? etag = null;

                await _retryPolicy.Retry(async () => etag = await InnerHandle(request, cancellationToken));

                await _ticketing.Complete(
                    request.TicketId,
                    new TicketResult(etag),
                    cancellationToken);
            }
            catch (IfMatchHeaderValueMismatchException)
            {
                await _ticketing.Error(
                    request.TicketId,
                    new TicketError("Als de If-Match header niet overeenkomt met de laatste ETag.", "PreconditionFailed"),
                    cancellationToken);
            }
            catch (DomainException exception)
            {
                var ticketError = exception switch
                {
                    BuildingUnitIsNotFoundException => new TicketError(
                        ValidationErrorMessages.Building.BuildingNotFound,
                        ValidationErrorCodes.Building.BuildingNotFound),
                    BuildingIsRemovedException => new TicketError(
                        ValidationErrorMessages.Building.BuildingRemoved,
                        ValidationErrorCodes.Building.BuildingRemoved),
                    _ => MapDomainException(exception, request)
                };

                ticketError ??= new TicketError(exception.Message, "");

                await _ticketing.Error(
                    request.TicketId,
                    ticketError,
                    cancellationToken);
            }

            return Unit.Value;
        }

        private async Task ValidateIfMatchHeaderValue(TSqsLambdaRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IfMatchHeaderValue) || request is not Abstractions.IHasBuildingPersistentLocalId id)
                return;

            var lastHash = await GetHash(
                new BuildingPersistentLocalId(id.BuildingPersistentLocalId),
                cancellationToken);

            var lastHashTag = new ETag(ETagType.Strong, lastHash);

            if (request.IfMatchHeaderValue != lastHashTag.ToString())
            {
                throw new IfMatchHeaderValueMismatchException();
            }
        }

        protected async Task<string> GetHash(
            BuildingPersistentLocalId buildingPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var aggregate =
                await _buildings.GetAsync(new BuildingStreamId(buildingPersistentLocalId), cancellationToken);
            return aggregate.LastEventHash;
        }
    }
}
