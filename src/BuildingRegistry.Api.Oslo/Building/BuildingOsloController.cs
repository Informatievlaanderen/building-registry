namespace BuildingRegistry.Api.Oslo.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Count;
    using Detail;
    using Infrastructure;
    using Infrastructure.Options;
    using List;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Query;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("gebouwen")]
    [ApiExplorerSettings(GroupName = "Gebouwen")]
    public class BuildingOsloController : ApiController
    {
        private readonly IMediator _mediator;

        public BuildingOsloController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Vraag een gebouw op.
        /// </summary>
        /// <param name="persistentLocalId"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als het gebouw gevonden is.</response>
        /// <response code="404">Als het gebouw niet gevonden kan worden.</response>
        /// <response code="410">Als het gebouw verwijderd werd.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{persistentLocalId}")]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(BuildingOsloResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(BuildingNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(BuildingGoneResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var response =
                await _mediator.Send(
                    new BuildingDetailRequest(persistentLocalId), cancellationToken);

            return string.IsNullOrWhiteSpace(response.LastEventHash)
                ? Ok(response.BuildingResponse)
                : new OkWithLastObservedPositionAsETagResult(response.BuildingResponse, response.LastEventHash);
        }

        /// <summary>
        /// Vraag een lijst met actieve gebouwn op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="responseOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met gebouwn gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(BuildingListOsloResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingListResponseOsloExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> List(
            [FromServices] LegacyContext context,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var listResponse = await _mediator.Send(
                new BuildingListRequest(
                    Request.ExtractFilteringRequest<BuildingFilter>(),
                    Request.ExtractSortingRequest(),
                    Request.ExtractPaginationRequest()),
                cancellationToken);

            Response.AddPaginationResponse(listResponse.Pagination);
            Response.AddSortingResponse(listResponse.Sorting);

            return Ok(listResponse);
        }

        /// <summary>
        /// Vraag het totaal aantal actieve gebouwen op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van het totaal aantal gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("totaal-aantal")]
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountOsloResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Count(
            [FromServices] LegacyContext context,
            CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new BuildingCountRequest(
                Request.ExtractFilteringRequest<BuildingFilter>(),
                Request.ExtractSortingRequest()
            ), cancellationToken);
            return Ok(response);
        }
    }
}
