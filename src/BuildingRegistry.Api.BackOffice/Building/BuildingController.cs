namespace BuildingRegistry.Api.BackOffice.Building
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Middleware;
    using BuildingRegistry.Building;
    using FluentValidation;
    using FluentValidation.Results;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("gebouwen")]
    [ApiExplorerSettings(GroupName = "gebouwen")]
    public partial class BuildingController : ApiController
    {
        private readonly IMediator _mediator;

        public BuildingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected IDictionary<string, object> GetMetadata()
        {
            var userId = User.FindFirst("urn:be:vlaanderen:buildingregistry:acmid")?.Value;
            var correlationId = User.FindFirst(AddCorrelationIdMiddleware.UrnBasisregistersVlaanderenCorrelationId)?.Value;

            return new Dictionary<string, object>
            {
                { "UserId", userId },
                { "CorrelationId", correlationId }
            };
        }

        private ValidationException CreateValidationException(string errorCode, string propertyName, string message)
        {
            var failure = new ValidationFailure(propertyName, message)
            {
                ErrorCode = errorCode
            };

            return new ValidationException(new List<ValidationFailure>
            {
                failure
            });
        }

        private async Task<ETag> GetEtag(
            IBuildings buildingRepository,
            int buildingPersistentLocalId,
            CancellationToken cancellationToken)
        {
            var aggregate =
                await buildingRepository.GetAsync(new BuildingStreamId(new BuildingPersistentLocalId(buildingPersistentLocalId)), cancellationToken);
            return new ETag(ETagType.Strong, aggregate.LastEventHash);
        }
    }
}
