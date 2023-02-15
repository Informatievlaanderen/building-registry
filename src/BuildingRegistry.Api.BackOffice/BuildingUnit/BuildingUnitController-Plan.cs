namespace BuildingRegistry.Api.BackOffice.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Validators;
    using Abstractions.BuildingUnit.Requests;
    using Abstractions.BuildingUnit.SqsRequests;
    using Abstractions.Validation;
    using Be.Vlaanderen.Basisregisters.AcmIdm;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentValidation;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingUnitController
    {
        /// <summary>
        /// Plan een gebouweenheid in.
        /// </summary>
        /// <param name="planBuildingUnitSqsRequestFactory"></param>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("acties/plannen")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string",
            "De url van de geplande gebouweenheid.")]
        [SwaggerRequestExample(typeof(PlanBuildingUnitRequest), typeof(PlanBuildingUnitRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
            Policy = PolicyNames.GeschetstGebouw.DecentraleBijwerker)]
        public async Task<IActionResult> Plan(
            [FromServices] IValidator<PlanBuildingUnitRequest> validator,
            [FromServices] PlanBuildingUnitSqsRequestFactory planBuildingUnitSqsRequestFactory,
            [FromBody] PlanBuildingUnitRequest request,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                OsloPuriValidator.TryParseIdentifier(request.GebouwId, out var buildingIdentifier);

                var result = await Mediator.Send(
                    planBuildingUnitSqsRequestFactory.Create(request, GetMetadata(), new ProvenanceData(CreateProvenance(Modification.Insert))),
                    cancellationToken);

                return Accepted(result);
            }
            catch (AggregateIdIsNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.BuildingNotFound.Message, StatusCodes.Status400BadRequest);
            }
            catch (AggregateNotFoundException)
            {
                throw new ApiException(ValidationErrors.Common.BuildingNotFound.Message, StatusCodes.Status400BadRequest);
            }
        }
    }
}
