namespace BuildingRegistry.Api.BackOffice.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.BuildingUnit.Requests;
    using Abstractions.BuildingUnit.SqsRequests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AcmIdm;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using BuildingRegistry.Building;
    using Infrastructure;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingUnitController
    {
        /// <summary>
        /// Realiseer een gebouweenheid niet.
        /// </summary>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="request"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("{buildingUnitPersistentLocalId}/acties/nietrealiseren")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De url van de niet gerealiseerde gebouweenheid.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.GeschetstGebouw.DecentraleBijwerker)]
        public async Task<IActionResult> NotRealize(
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromRoute] NotRealizeBuildingUnitRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken ct = default)
        {
            try
            {
                if (!await ifMatchHeaderValidator
                        .IsValidForBuildingUnit(ifMatchHeaderValue,
                            new BuildingUnitPersistentLocalId(request.BuildingUnitPersistentLocalId), ct))
                {
                    return new PreconditionFailedResult();
                }

                var result = await Mediator.Send(
                    new NotRealizeBuildingUnitSqsRequest
                    {
                        Request = request,
                        Metadata = GetMetadata(),
                        ProvenanceData = new ProvenanceData(CreateProvenance(Modification.Update)),
                        IfMatchHeaderValue = ifMatchHeaderValue
                    }, ct);

                return Accepted(result);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.BuildingUnitNotFound.Message,
                    StatusCodes.Status404NotFound);
            }
            catch (AggregateNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.BuildingUnitNotFound.Message, StatusCodes.Status404NotFound);
            }
        }
    }
}
