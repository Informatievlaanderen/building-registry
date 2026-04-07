namespace BuildingRegistry.Api.Oslo.Building
{
    using System;
    using System.Linq;
    using System.Net.Mime;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.Api.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using ChangeFeed;
    using CloudNative.CloudEvents;
    using Count;
    using Detail;
    using Infrastructure;
    using Infrastructure.Options;
    using List;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Projections.Feed;
    using Projections.Legacy;
    using Query;
    using Swashbuckle.AspNetCore.Filters;
    using Sync;
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
            var response = await _mediator.Send(new BuildingCountRequest(
                Request.ExtractFilteringRequest<BuildingFilter>(),
                Request.ExtractSortingRequest()
            ), cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Vraag een lijst met wijzigingen van gebouwen op.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="responseOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("sync")]
        [Produces("text/xml")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingSyndicationResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Sync(
            [FromServices] IConfiguration configuration,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new SyncRequest(
                Request.ExtractFilteringRequest<BuildingSyndicationFilter>(),
                Request.ExtractSortingRequest(),
                Request.ExtractPaginationRequest(maxLimit:100)
            ), cancellationToken);

            return new ContentResult
            {
                Content = await BuildAtomFeed(response.LastFeedUpdate, response.PagedBuildings, responseOptions, configuration),
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Vraag een lijst met wijzigingen van gebouwen op (CloudEvents).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="page"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("wijzigingen")]
        [Produces(AcceptTypes.JsonCloudEventsBatch)]
        [ProducesResponseType(typeof(System.Collections.Generic.List<CloudEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingFeedResultExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Changes(
            [FromServices] FeedContext context,
            [FromQuery] int? page,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<BuildingFeedFilter>();
            if (page is null)
                page = filtering.Filter?.Page ?? 1;

            var feedPosition = filtering.Filter?.FeedPosition;

            if (feedPosition.HasValue && filtering.Filter?.Page.HasValue == false)
            {
                page = context.BuildingFeed
                    .Where(x => x.Position == feedPosition.Value)
                    .Select(x => x.Page)
                    .Distinct()
                    .AsEnumerable()
                    .DefaultIfEmpty(1)
                    .Min();
            }

            var feedItemsEvents = await context
                .BuildingFeed
                .Where(x => x.Page == page)
                .OrderBy(x => x.Id)
                .Select(x => x.CloudEventAsString)
                .ToListAsync(cancellationToken);

            var jsonContent = "[" + string.Join(",", feedItemsEvents) + "]";

            return new ChangeFeedResult(jsonContent, feedItemsEvents.Count >= ChangeFeedService.DefaultMaxPageSize);
        }

        /// <summary>
        /// Vraag wijzigingen van een bepaald gebouw op (CloudEvents).
        /// </summary>
        /// <param name="context"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{persistentLocalId}/wijzigingen")]
        [Produces(AcceptTypes.JsonCloudEventsBatch)]
        [ProducesResponseType(typeof(System.Collections.Generic.List<CloudEvent>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingFeedResultExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> ChangesByPersistentLocalId(
            [FromServices] FeedContext context,
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var pagination = (PaginationRequest)Request.ExtractPaginationRequest();

            var feedItemsEvents = await context
                .BuildingFeed
                .Where(x => x.BuildingPersistentLocalId == persistentLocalId)
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
            var filtering = Request.ExtractFilteringRequest<BuildingPositionFilter>();
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
                        .BuildingFeed
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
                        .BuildingFeed
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
                        .BuildingFeed
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

        private static async Task<string> BuildAtomFeed(
            DateTimeOffset lastUpdate,
            PagedQueryable<BuildingSyndicationQueryResult> pagedBuildings,
            IOptions<ResponseOptions> responseOptions,
            IConfiguration configuration)
        {
            var sw = new StringWriterWithEncoding(Encoding.UTF8);

            using (var xmlWriter = XmlWriter.Create(sw,
                       new XmlWriterSettings {Async = true, Indent = true, Encoding = sw.Encoding}))
            {
                var formatter = new AtomFormatter(null, xmlWriter.Settings) {UseCDATA = true};
                var writer = new AtomFeedWriter(xmlWriter, null, formatter);
                var syndicationConfiguration = configuration.GetSection("Syndication");
                var atomConfiguration = AtomFeedConfigurationBuilder.CreateFrom(syndicationConfiguration, lastUpdate);

                await writer.WriteDefaultMetadata(atomConfiguration);

                var buildings = pagedBuildings.Items.ToList();

                var nextFrom = buildings.Any()
                    ? buildings.Max(x => x.Position) + 1
                    : (long?) null;

                var nextUri = BuildNextSyncUri(pagedBuildings.PaginationInfo.Limit, nextFrom,
                    syndicationConfiguration["NextUri"]!);
                if (nextUri is not null)
                {
                    await writer.Write(new SyndicationLink(nextUri, "next"));
                }

                foreach (var building in pagedBuildings.Items)
                {
                    await writer.WriteBuilding(
                        responseOptions,
                        formatter,
                        syndicationConfiguration["Category1"]!,
                        syndicationConfiguration["Category2"]!,
                        building);
                }

                await xmlWriter.FlushAsync();
            }

            return sw.ToString();
        }

        private static Uri? BuildNextSyncUri(int limit, long? from, string nextUrlBase) => from.HasValue
            ? new Uri(string.Format(nextUrlBase, from, limit))
            : null;
    }
}
