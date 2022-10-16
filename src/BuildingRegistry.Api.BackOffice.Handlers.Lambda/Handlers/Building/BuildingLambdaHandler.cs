namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
    using System.Configuration;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Infrastructure;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Requests.Building;
    using TicketingService.Abstractions;

    public abstract class BuildingLambdaHandler<TSqsLambdaRequest> : IRequestHandler<TSqsLambdaRequest>
        where TSqsLambdaRequest : BuildingLambdaRequest
    {
        private readonly ITicketing _ticketing;
        private readonly ICustomRetryPolicy _retryPolicy;
        private readonly IBuildings _buildings;

        protected IIdempotentCommandHandler IdempotentCommandHandler { get; }
        protected string DetailUrlFormat { get; }

        protected BuildingLambdaHandler(
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

            DetailUrlFormat = configuration["BuildingDetailUrl"];
            if (string.IsNullOrEmpty(DetailUrlFormat))
            {
                throw new ConfigurationErrorsException("'BuildingDetailUrl' cannot be found in the configuration");
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

                ETagResponse? eTag = null;

                await _retryPolicy.Retry(async () => eTag = await InnerHandle(request, cancellationToken));

                await _ticketing.Complete(
                    request.TicketId,
                    new TicketResult(eTag),
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
            if (string.IsNullOrWhiteSpace(request.IfMatchHeaderValue) ||
                request is not Abstractions.IHasBuildingPersistentLocalId id)
            {
                return;
            }

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
