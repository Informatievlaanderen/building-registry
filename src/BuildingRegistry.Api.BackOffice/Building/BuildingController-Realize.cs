namespace BuildingRegistry.Api.BackOffice.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Abstractions.Building.Validators;
    using Abstractions.Building.SqsRequests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using Infrastructure;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingController
    {
        /// <summary>
        /// Gebouw realiseren.
        /// </summary>
        /// <param name="buildingExistsValidator"></param>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="request"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">Aanvraag tot goedkeuring wordt reeds verwerkt.</response>
        /// <response code="412">Als de If-Match header niet overeenkomt met de laatste ETag.</response>
        /// <returns></returns>
        [HttpPost("{persistentLocalId}/acties/realiseren")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De url van het gebouw.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Policy = PolicyNames.GeschetstGebouw.DecentraleBijwerker)]
        public async Task<IActionResult> Realize(
            [FromServices] BuildingExistsValidator buildingExistsValidator,
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromRoute] RealizeBuildingRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            if (!await buildingExistsValidator.Exists(new BuildingPersistentLocalId(request.PersistentLocalId),
                    cancellationToken))
            {
                throw new ApiException(ValidationErrors.Common.BuildingNotFound.Message, StatusCodes.Status404NotFound);
            }

            if (!await ifMatchHeaderValidator.IsValidForBuilding(ifMatchHeaderValue,
                    new BuildingPersistentLocalId(request.PersistentLocalId), cancellationToken))
            {
                return new PreconditionFailedResult();
            }

            var result = await Mediator.Send(
                new RealizeBuildingSqsRequest
                {
                    Request = request,
                    Metadata = GetMetadata(),
                    ProvenanceData = new ProvenanceData(CreateProvenance(Modification.Update)),
                    IfMatchHeaderValue = ifMatchHeaderValue
                }, cancellationToken);

            return Accepted(result);
        }
    }
}
