namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Requests.BuildingUnit;
    using TicketingService.Abstractions;

    public sealed class AttachAddressToBuildingUnitLambdaHandler : BuildingUnitLambdaHandler<AttachAddressToBuildingUnitLambdaRequest>
    {
        private readonly BackOfficeContext _backOfficeContext;

        public AttachAddressToBuildingUnitLambdaHandler(
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

        protected override async Task<ETagResponse> InnerHandle(AttachAddressToBuildingUnitLambdaRequest request, CancellationToken cancellationToken)
        {
            var cmd = request.ToCommand();

            try
            {
                await IdempotentCommandHandler.Dispatch(
                    cmd.CreateCommandId(),
                    cmd,
                    request.Metadata,
                    cancellationToken);

                _backOfficeContext.BuildingUnitAddressRelation.Add(new BuildingUnitAddressRelation(cmd.BuildingPersistentLocalId, cmd.BuildingUnitPersistentLocalId, cmd.AddressPersistentLocalId));
                await _backOfficeContext.SaveChangesAsync(cancellationToken);
            }
            catch (IdempotencyException)
            {
                // Idempotent: Do Nothing return last etag
            }

            var lastHash = await GetHash(
                request.BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(request.BuildingUnitPersistentLocalId),
                cancellationToken);

            return new ETagResponse(string.Format(DetailUrlFormat, request.BuildingUnitPersistentLocalId), lastHash);
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, AttachAddressToBuildingUnitLambdaRequest request)
        {
            return exception switch
            {
                BuildingUnitHasInvalidStatusException =>
                        ValidationErrors.AttachAddressToBuildingUnit.BuildingUnitInvalidStatus.ToTicketError(),

                AddressNotFoundException =>
                    ValidationErrors.Common.AdresIdInvalid.ToTicketError(),

                AddressIsRemovedException =>
                    ValidationErrors.Common.AdresIdInvalid.ToTicketError(),

                AddressHasInvalidStatusException =>
                    ValidationErrors.AttachAddressToBuildingUnit.AddressInvalidStatus.ToTicketError(),

                _ => null
            };
        }
    }
}
