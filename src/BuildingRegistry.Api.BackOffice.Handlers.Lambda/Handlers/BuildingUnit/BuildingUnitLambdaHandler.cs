namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit
{
    using System.Configuration;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using Abstractions.Exceptions;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using BuildingRegistry.Infrastructure;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Requests.BuildingUnit;
    using TicketingService.Abstractions;

    public abstract class BuildingUnitLambdaHandler<TSqsLambdaRequest> : IRequestHandler<TSqsLambdaRequest>
        where TSqsLambdaRequest : BuildingUnitLambdaRequest
    {
        private readonly ITicketing _ticketing;
        private readonly ICustomRetryPolicy _retryPolicy;
        private readonly IBuildings _buildings;

        protected IIdempotentCommandHandler IdempotentCommandHandler { get; }
        protected string DetailUrlFormat { get; }

        protected BuildingUnitLambdaHandler(
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

            DetailUrlFormat = configuration["BuildingUnitDetailUrl"];
            if (string.IsNullOrEmpty(DetailUrlFormat))
            {
                throw new ConfigurationErrorsException("'BuildingUnitDetailUrl' cannot be found in the configuration");
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
            if (string.IsNullOrWhiteSpace(request.IfMatchHeaderValue) || request is not Abstractions.IHasBuildingUnitPersistentLocalId id)
            {
                return;
            }

            var lastHash = await GetHash(
                request.BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(id.BuildingUnitPersistentLocalId),
                cancellationToken);

            var lastHashTag = new ETag(ETagType.Strong, lastHash);

            if (request.IfMatchHeaderValue != lastHashTag.ToString())
            {
                throw new IfMatchHeaderValueMismatchException();
            }
        }

        protected async Task<string> GetHash(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var aggregate =
                await _buildings.GetAsync(new BuildingStreamId(new BuildingPersistentLocalId(buildingPersistentLocalId)), cancellationToken);

            var buildingUnit = aggregate.BuildingUnits.Single(
                x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

            return buildingUnit.LastEventHash;
        }
    }
}
