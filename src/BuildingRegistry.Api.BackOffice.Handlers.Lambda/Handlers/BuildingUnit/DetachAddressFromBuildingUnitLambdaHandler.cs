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

    public sealed class DetachAddressFromBuildingUnitLambdaHandler : BuildingUnitLambdaHandler<DetachAddressFromBuildingUnitLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public DetachAddressFromBuildingUnitLambdaHandler(
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

        protected override async Task<object> InnerHandle(DetachAddressFromBuildingUnitLambdaRequest request,
            CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

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

            await _backOfficeContext.RemoveIdempotentBuildingUnitAddressRelation(cmd.BuildingUnitPersistentLocalId, cmd.AddressPersistentLocalId, cancellationToken);

            var lastHash = await GetHash(
                request.BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(request.BuildingUnitPersistentLocalId),
                cancellationToken);

            return new ETagResponse(string.Format(DetailUrlFormat, request.BuildingUnitPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception,
            DetachAddressFromBuildingUnitLambdaRequest request)
        {
            return exception switch
            {
                AddressNotFoundException =>
                    ValidationErrors.Common.AdresIdInvalid.ToTicketError(),

                AddressIsRemovedException =>
                    ValidationErrors.Common.AdresIdInvalid.ToTicketError(),

                _ => null
            };
        }
    }
}
