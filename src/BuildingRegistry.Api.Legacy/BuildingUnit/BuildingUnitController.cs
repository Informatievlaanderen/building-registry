namespace BuildingRegistry.Api.Legacy.BuildingUnit
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
    using List;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Query;
    using Swashbuckle.AspNetCore.Filters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("gebouweenheden")]
    [ApiExplorerSettings(GroupName = "Gebouweenheden")]
    public class BuildingUnitController : ApiController
    {
        private readonly IMediator _mediator;

        public BuildingUnitController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Vraag een lijst met actieve gebouweenheden op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van een lijst met gebouweenheden gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(BuildingUnitListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingUnitListResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> List(CancellationToken cancellationToken = default)
        {
            var listResponse = await _mediator.Send(
                new BuildingUnitListRequest(
                    Request.ExtractFilteringRequest<BuildingUnitFilter>(),
                    Request.ExtractSortingRequest(),
                    Request.ExtractPaginationRequest()),
                cancellationToken);

            Response.AddPaginationResponse(listResponse.Pagination);
            Response.AddSortingResponse(listResponse.Sorting);

            return Ok(listResponse);
        }

        /// <summary>
        /// Vraag het totaal aantal actieve gebouweenheden op.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van het totaal aantal gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("totaal-aantal")]
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Count(CancellationToken cancellationToken = default)
        {
            var response =
                await _mediator.Send(new CountRequest(
                    Request.ExtractFilteringRequest<BuildingUnitFilter>(),
                    Request.ExtractSortingRequest()
                    ), cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Vraag een gebouweenheid op.
        /// </summary>
        /// <param name="persistentLocalId"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de gebouweenheid gevonden is.</response>
        /// <response code="404">Als de gebouweenheid niet gevonden kan worden.</response>
        /// <response code="410">Als de gebouweenheid verwijderd werd.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{persistentLocalId}")]
        [ProducesResponseType(typeof(BuildingUnitResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingUnitResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(BuildingUnitNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(BuildingUnitGoneResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var response =
                await _mediator.Send(
                    new GetBuildingUnitDetailRequest(persistentLocalId),
                    cancellationToken);

            return string.IsNullOrWhiteSpace(response.LastEventHash)
                ? Ok(response.BuildingUnitResponse)
                : new OkWithLastObservedPositionAsETagResult(response.BuildingUnitResponse, response.LastEventHash);
        }
    }
}
