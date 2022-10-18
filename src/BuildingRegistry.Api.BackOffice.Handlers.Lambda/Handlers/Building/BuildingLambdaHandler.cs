namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building
{
    using System.Configuration;
    using Abstractions.Building.Validators;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.Building;
    using TicketingService.Abstractions;

    public abstract class BuildingLambdaHandler<TSqsLambdaRequest> : SqsLambdaHandlerBase<TSqsLambdaRequest>
        where TSqsLambdaRequest : BuildingLambdaRequest
    {
        private readonly IBuildings _buildings;

        protected string DetailUrlFormat { get; }

        protected BuildingLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings)
            : base(retryPolicy, ticketing, idempotentCommandHandler)
        {
            _buildings = buildings;

            DetailUrlFormat = configuration["BuildingDetailUrl"];
            if (string.IsNullOrEmpty(DetailUrlFormat))
            {
                throw new ConfigurationErrorsException("'BuildingDetailUrl' cannot be found in the configuration");
            }
        }

        protected override async Task ValidateIfMatchHeaderValue(TSqsLambdaRequest request, CancellationToken cancellationToken)
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

        protected override Task HandleAggregateIdIsNotFoundException(
            TSqsLambdaRequest request,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected abstract TicketError? InnerMapDomainException(DomainException exception, TSqsLambdaRequest request);

        protected override TicketError? MapDomainException(DomainException exception, TSqsLambdaRequest request)
        {
            var error = InnerMapDomainException(exception, request);
            if (error is not null)
            {
                return error;
            }

            return exception switch
            {
                BuildingIsRemovedException => new TicketError(
                    ValidationErrorMessages.Building.BuildingRemoved,
                    ValidationErrorCodes.Building.BuildingRemoved),
                _ => null
            };
        }
    }
}
