namespace BuildingRegistry.Api.BackOffice.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Abstractions.Building.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using NodaTime;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingController
    {
        /// <summary>
        /// Stel een gebouw vast.
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="sqsRequestFactory"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        [HttpPost("acties/vaststellen")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(RealizeAndMeasureUnplannedBuildingRequest), typeof(RealizeAndMeasureUnplannedBuildingRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> RealizeAndMeasureUnplannedBuilding(
            [FromServices] IValidator<RealizeAndMeasureUnplannedBuildingRequest> validator,
            [FromServices] RealizeAndMeasureUnplannedBuildingSqsRequestFactory sqsRequestFactory,
            [FromBody] RealizeAndMeasureUnplannedBuildingRequest request,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var result = await Mediator.Send(
                sqsRequestFactory.Create(request, GetMetadata(), new ProvenanceData(new Provenance(
                    SystemClock.Instance.GetCurrentInstant(),
                    Application.Grb,
                    new Reason(""),
                    new Operator(""),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen))),
                cancellationToken);

            return Accepted(result);
        }
    }
}
