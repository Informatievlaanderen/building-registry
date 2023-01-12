namespace BuildingRegistry.Api.BackOffice.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Validators;
    using Abstractions.BuildingUnit.Requests;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Handlers.Sqs.Requests.BuildingUnit;
    using Infrastructure;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingUnitController
    {
        /// <summary>
        /// Corrigeer de deregularisatie van een gebouweenheid.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="request"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("{buildingUnitPersistentLocalId}/acties/corrigeren/deregularisatie")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De URL van het aangemaakte ticket.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> CorrectDeregulation(
            [FromServices] IOptions<ResponseOptions> options,
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromRoute] CorrectBuildingUnitDeregulationRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken ct = default)
        {
            try
            {
                if (!await ifMatchHeaderValidator
                        .IsValidForBuildingUnit(ifMatchHeaderValue, new BuildingUnitPersistentLocalId(request.BuildingUnitPersistentLocalId), ct))
                {
                    return new PreconditionFailedResult();
                }

                var result = await Mediator.Send(
                    new CorrectBuildingUnitDeregulationSqsRequest()
                    {
                        Request = request,
                        Metadata = GetMetadata(),
                        ProvenanceData = new ProvenanceData(CreateFakeProvenance()),
                        IfMatchHeaderValue = ifMatchHeaderValue
                    }, ct);

                return Accepted(result);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw new ApiException(ValidationErrorMessages.BuildingUnit.BuildingUnitNotFound, StatusCodes.Status404NotFound);
            }
            catch (BuildingUnitIsNotFoundException)
            {
                throw new ApiException(ValidationErrorMessages.BuildingUnit.BuildingUnitNotFound, StatusCodes.Status404NotFound);
            }
        }
    }
}
