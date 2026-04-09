namespace BuildingRegistry.Api.Oslo.BuildingUnit
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Building.ChangeFeed;
    using ChangeFeed;
    using CloudNative.CloudEvents;
    using Count;
    using Detail;
    using Infrastructure;
    using List;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Projections.Feed;
    using Projections.Legacy;
    using Query;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("gebouweenheden")]
    [ApiExplorerSettings(GroupName = "Gebouweenheden")]
    public class BuildingUnitOsloController : ApiController
    {
        private readonly IMediator _mediator;

        public BuildingUnitOsloController(IMediator mediator)
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
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(BuildingUnitListOsloResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingUnitListOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> List(
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<BuildingUnitFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var listResponse = await _mediator.Send(new ListRequest(filtering, sorting, pagination), cancellationToken);

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
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountOsloResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Count(
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<BuildingUnitFilter>();
            var sorting = Request.ExtractSortingRequest();

            var response = await _mediator.Send(new CountRequest(filtering, sorting), cancellationToken);

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
        [Produces(AcceptTypes.JsonLd)]
        [ProducesResponseType(typeof(BuildingUnitOsloResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingUnitOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(BuildingUnitNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(BuildingUnitGoneResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new GetRequest(persistentLocalId), cancellationToken);

            return string.IsNullOrWhiteSpace(response.LastEventHash)
                ? Ok(response.BuildingUnitResponse)
                : new OkWithLastObservedPositionAsETagResult(response.BuildingUnitResponse, response.LastEventHash);
        }

        /// <summary>
        /// Vraag een lijst met wijzigingen van gebouweenheden op (CloudEvents).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="page"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("wijzigingen")]
        [Produces(AcceptTypes.JsonCloudEventsBatch)]
        [ProducesResponseType(typeof(System.Collections.Generic.List<CloudEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingUnitFeedResultExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Changes(
            [FromServices] FeedContext context,
            [FromQuery] int? page,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<BuildingUnitFeedFilter>();
            if (page is null)
                page = filtering.Filter?.Page ?? 1;

            var feedPosition = filtering.Filter?.FeedPosition;

            if (feedPosition.HasValue && filtering.Filter?.Page.HasValue == false)
            {
                page = context.BuildingUnitFeed
                    .Where(x => x.Position == feedPosition.Value)
                    .Select(x => x.Page)
                    .Distinct()
                    .AsEnumerable()
                    .DefaultIfEmpty(1)
                    .Min();
            }

            var feedItemsEvents = await context
                .BuildingUnitFeed
                .Where(x => x.Page == page)
                .OrderBy(x => x.Id)
                .Select(x => x.CloudEventAsString)
                .ToListAsync(cancellationToken);

            var jsonContent = "[" + string.Join(",", feedItemsEvents) + "]";

            return new ChangeFeedResult(jsonContent, feedItemsEvents.Count >= ChangeFeedService.DefaultMaxPageSize);
        }

        /// <summary>
        /// Vraag wijzigingen van een bepaalde gebouweenheid op (CloudEvents).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{persistentLocalId}/wijzigingen")]
        [Produces(AcceptTypes.JsonCloudEventsBatch)]
        [ProducesResponseType(typeof(System.Collections.Generic.List<CloudEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingUnitFeedResultExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> ChangesByPersistentLocalId(
            [FromServices] FeedContext context,
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var pagination = (PaginationRequest)Request.ExtractPaginationRequest();

            var feedItemsEvents = await context
                .BuildingUnitFeed
                .Where(x => x.BuildingUnitPersistentLocalId == persistentLocalId)
                .OrderBy(x => x.Id)
                .Select(x => x.CloudEventAsString)
                .Skip(pagination.Offset)
                .Take(pagination.Limit)
                .ToListAsync(cancellationToken);

            var jsonContent = "[" + string.Join(",", feedItemsEvents) + "]";

            return Content(jsonContent, AcceptTypes.JsonCloudEventsBatch);
        }

        [HttpGet("posities")]
        [Produces(AcceptTypes.Json)]
        [ProducesResponseType(typeof(FeedPositieResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPositions(
            [FromServices] LegacyContext legacyContext,
            [FromServices] FeedContext feedContext,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<BuildingUnitPositionFilter>();
            var response = new FeedPositieResponse();
            if (filtering.ShouldFilter && !filtering.Filter.HasMoreThanOneFilter)
            {
                if (filtering.Filter.Download.HasValue)
                {
                    var businessFeedPosition = await legacyContext
                        .BuildingSyndicationWithCount
                        .AsNoTracking()
                        .Where(x => x.Position <= filtering.Filter.Download.Value)
                        .OrderByDescending(x => x.Position)
                        .Select(x => x.Position)
                        .FirstOrDefaultAsync(cancellationToken);

                    var changeFeed = await feedContext
                        .BuildingUnitFeed
                        .AsNoTracking()
                        .Where(x => x.Position <= filtering.Filter.Download.Value)
                        .OrderByDescending(x => x.Position)
                        .Select(x => new { x.Id, x.Page })
                        .FirstOrDefaultAsync(cancellationToken);

                    response.Feed = businessFeedPosition;
                    response.WijzigingenFeedPagina = changeFeed?.Page;
                    response.WijzigingenFeedId = changeFeed?.Id;
                }
                else if (filtering.Filter.Sync.HasValue)
                {
                    var position = await legacyContext
                        .BuildingSyndicationWithCount
                        .AsNoTracking()
                        .Where(x => x.Position <= filtering.Filter.Sync.Value)
                        .OrderByDescending(x => x.Position)
                        .Select(x => x.Position)
                        .FirstOrDefaultAsync(cancellationToken);

                    var changeFeed = await feedContext
                        .BuildingUnitFeed
                        .AsNoTracking()
                        .Where(x => x.Position <= position)
                        .OrderByDescending(x => x.Position)
                        .Select(x => new { x.Id, x.Page })
                        .FirstOrDefaultAsync(cancellationToken);

                    response.Feed = filtering.Filter.Sync.Value;
                    response.WijzigingenFeedPagina = changeFeed?.Page;
                    response.WijzigingenFeedId = changeFeed?.Id;
                }
                else if (filtering.Filter.ChangeFeedId.HasValue)
                {
                    var feedItem = await feedContext
                        .BuildingUnitFeed
                        .AsNoTracking()
                        .Where(x => x.Id == filtering.Filter.ChangeFeedId.Value)
                        .Select(x => new { x.Id, x.Page, x.Position })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (feedItem is null)
                        return Ok(response);

                    var syncPosition = await legacyContext
                        .BuildingSyndicationWithCount
                        .AsNoTracking()
                        .Where(x => x.Position == feedItem.Position)
                        .OrderByDescending(x => x.Position)
                        .Select(x => x.Position)
                        .FirstOrDefaultAsync(cancellationToken);

                    response.Feed = syncPosition;
                    response.WijzigingenFeedPagina = feedItem.Page;
                    response.WijzigingenFeedId = feedItem.Id;
                }
            }

            return Ok(response);
        }
    }
}
