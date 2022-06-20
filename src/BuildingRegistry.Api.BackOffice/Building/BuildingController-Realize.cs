namespace BuildingRegistry.Api.BackOffice.Building
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Building;
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
        public async Task<IActionResult> Realize(
            [FromServices] IBuildings buildingsRepository,
            [FromServices] IOptions<ResponseOptions> options,
            [FromServices] IValidator<RealizeBuildingRequest> validator,
            [FromRoute] RealizeBuildingRequest request,
            [FromHeader(Name = "If-Match")] string? ifMatchHeaderValue,
            CancellationToken cancellationToken = default)
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                request.Metadata = GetMetadata();

                // Check if user provided ETag is equal to the current Entity Tag
                if (ifMatchHeaderValue is not null)
                {
                    var ifMatchTag = ifMatchHeaderValue.Trim();
                    var currentETag = await GetEtag(buildingsRepository, request.PersistentLocalId, cancellationToken);
                    if (ifMatchTag != currentETag.ToString())
                    {
                        return new PreconditionFailedResult();
                    }
                }

                var response = await _mediator.Send(request, cancellationToken);

                return new AcceptedWithETagResult(
                    new Uri(string.Format(options.Value.BuildingDetailUrl, request.PersistentLocalId)),
                    response.LastEventHash);
            }
            catch (IdempotencyException)
            {
                return Accepted();
            }
        }
    }
}
