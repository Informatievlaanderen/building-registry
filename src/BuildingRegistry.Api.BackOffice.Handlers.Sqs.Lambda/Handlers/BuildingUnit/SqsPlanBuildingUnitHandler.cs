namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers.BuildingUnit
{
    using Abstractions.Building.Responses;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.Extensions.Configuration;
    using Requests.BuildingUnit;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Infrastructure;
    using TicketingService.Abstractions;

    public sealed class SqsPlanBuildingUnitHandler : SqsLambdaBuildingUnitHandler<SqsLambdaBuildingUnitPlanRequest>
    {
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;

        public SqsPlanBuildingUnitHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IBuildings buildings,
            IPersistentLocalIdGenerator persistentLocalIdGenerator)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                buildings)
        {
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
        }

        protected override async Task<ETagResponse> InnerHandle(SqsLambdaBuildingUnitPlanRequest request, CancellationToken cancellationToken)
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(OsloPuriValidatorExtensions.ParsePersistentLocalId(request.Request.GebouwId));
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(_persistentLocalIdGenerator.GenerateNextPersistentLocalId());

            var cmd = request.ToCommand(buildingPersistentLocalId, buildingUnitPersistentLocalId);

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

            var lastHash = await GetHash(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                cancellationToken);
            return new ETagResponse(lastHash);
        }

        protected override TicketError? MapDomainException(DomainException exception, SqsLambdaBuildingUnitPlanRequest request)
        {
            return exception switch
            {
                BuildingHasInvalidStatusException => new TicketError(
                    ValidationErrorMessages.BuildingUnit.BuildingUnitCannotBePlanned,
                        ValidationErrorCodes.BuildingUnit.BuildingUnitCannotBePlanned),

                BuildingUnitPositionIsOutsideBuildingGeometryException => new TicketError(
                        ValidationErrorMessages.BuildingUnit.BuildingUnitOutsideGeometryBuilding,
                        ValidationErrorCodes.BuildingUnit.BuildingUnitOutsideGeometryBuilding),
                _ => null
            };
        }
    }
}
