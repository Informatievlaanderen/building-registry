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
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingController
    {
        /// <summary>
        /// Voeg gebouwen samen.
        /// </summary>
        /// <param name="validator"></param>
        /// <param name="mergeBuildingsSqsRequestFactory"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        [HttpPost("acties/samenvoegen")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(MergeBuildingRequest), typeof(MergeBuildingRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Merge(
            [FromServices] IValidator<MergeBuildingRequest> validator,
            [FromServices] MergeBuildingsSqsRequestFactory mergeBuildingsSqsRequestFactory,
            [FromBody] MergeBuildingRequest request,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            var result = await Mediator.Send(
                mergeBuildingsSqsRequestFactory.Create(request, GetMetadata(), new ProvenanceData(CreateProvenance(Modification.Insert))),
                cancellationToken);

            return Accepted(result);
        }
    }
}
