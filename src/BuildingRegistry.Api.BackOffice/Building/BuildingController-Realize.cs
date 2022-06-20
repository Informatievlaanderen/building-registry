namespace BuildingRegistry.Api.BackOffice.Building
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using FluentValidation;
    using Handlers.Building;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;

    public partial class BuildingController
    {
        /// <summary>
        /// Gebouw realiseren.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="202">todo</response>
        /// <returns></returns>
        [HttpPost("{persistentLocalId}/acties/realiseren")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseHeader(StatusCodes.Status202Accepted, "location", "string", "De url van het gebouw.")]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Realize(
            [FromServices] IOptions<ResponseOptions> options,
            [FromServices] IValidator<RealizeBuildingRequest> validator,
            [FromRoute] RealizeBuildingRequest request,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                request.Metadata = GetMetadata();
                var response = await _mediator.Send(request, cancellationToken);

                return new AcceptedWithETagResult(
                    new Uri(string.Format(options.Value.BuildingDetailUrl)),
                    response.LastEventHash);
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
        }
    }
}
