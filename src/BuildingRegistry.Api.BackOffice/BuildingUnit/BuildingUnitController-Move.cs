namespace BuildingRegistry.Api.BackOffice.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.BuildingUnit.Requests;
    using Abstractions.BuildingUnit.SqsRequests;
    using Abstractions.BuildingUnit.Validators;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using BuildingRegistry.Building;
    using FluentValidation;
    using Infrastructure;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingUnitController
    {
        /// <summary>
        /// Verplaats een gebouweenheid.
        /// </summary>
        /// <param name="ifMatchHeaderValidator"></param>
        /// <param name="validator"></param>
        /// <param name="buildingUnitPersistentLocalId"></param>
        /// <param name="request"></param>
        /// <param name="ifMatchHeaderValue"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("{buildingUnitPersistentLocalId}/acties/verplaatsen")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(MoveBuildingUnitRequest), typeof(MoveBuildingUnitRequestExamples))]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De url van de gebouweenheid.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.GeschetstGebouw.InterneBijwerker)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.IngemetenGebouw.InterneBijwerker)]
        public async Task<IActionResult> Move(
            [FromServices] IIfMatchHeaderValidator ifMatchHeaderValidator,
            [FromServices] IValidator<MoveBuildingUnitExtendedRequest> validator,
            [FromRoute] int buildingUnitPersistentLocalId,
            [FromBody] MoveBuildingUnitRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken ct = default)
        {
            await validator.ValidateAndThrowAsync(new MoveBuildingUnitExtendedRequest(request, buildingUnitPersistentLocalId), ct);

            try
            {

                if (!await ifMatchHeaderValidator
                        .IsValidForBuildingUnit(ifMatchHeaderValue, new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId), ct))
                {
                    return new PreconditionFailedResult();
                }

                var result = await Mediator.Send(
                    new MoveBuildingUnitSqsRequest
                    {
                        BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId,
                        Request = request,
                        Metadata = GetMetadata(),
                        ProvenanceData = new ProvenanceData(CreateProvenance(Modification.Update)),
                        IfMatchHeaderValue = ifMatchHeaderValue
                    }, ct);

                return Accepted(result);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.BuildingUnitNotFound.Message, StatusCodes.Status404NotFound);
            }
            catch (AggregateNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.BuildingUnitNotFound.Message, StatusCodes.Status404NotFound);
            }
        }
    }
}
