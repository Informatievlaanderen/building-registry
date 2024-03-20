namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit
{
    using System.Configuration;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.BuildingUnit;
    using TicketingService.Abstractions;
    using IHasBuildingUnitPersistentLocalId = Abstractions.IHasBuildingUnitPersistentLocalId;

    public abstract class BuildingUnitLambdaHandler<TSqsLambdaRequest> : SqsLambdaHandlerBase<TSqsLambdaRequest>
        where TSqsLambdaRequest : BuildingUnitLambdaRequest
    {
        private readonly IBuildings _buildings;

        protected string DetailUrlFormat { get; }

        protected BuildingUnitLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings)
            : base(retryPolicy, ticketing, idempotentCommandHandler)
        {
            _buildings = buildings;

            DetailUrlFormat = configuration["BuildingUnitDetailUrl"];
            if (string.IsNullOrEmpty(DetailUrlFormat))
            {
                throw new ConfigurationErrorsException("'BuildingUnitDetailUrl' cannot be found in the configuration");
            }
        }

        protected override async Task ValidateIfMatchHeaderValue(TSqsLambdaRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.IfMatchHeaderValue) || request is not IHasBuildingUnitPersistentLocalId id)
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

        protected override async Task HandleAggregateIdIsNotFoundException(
            TSqsLambdaRequest request,
            CancellationToken cancellationToken)
        {
            await Ticketing.Error(request.TicketId, ValidationErrors.Common.BuildingNotFound.ToTicketError(), cancellationToken);
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
                BuildingIsRemovedException => ValidationErrors.Common.BuildingIsRemoved.ToTicketError(),
                BuildingUnitIsRemovedException => ValidationErrors.Common.BuildingUnitIsRemoved.ToTicketError(),
                BuildingUnitIsNotFoundException => ValidationErrors.Common.BuildingUnitNotFound.ToTicketError(),
                _ => null
            };
        }
    }
}
