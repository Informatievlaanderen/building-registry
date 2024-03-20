namespace BuildingRegistry.Api.Legacy.Building
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
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.Api.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Count;
    using Crab;
    using Detail;
    using Infrastructure;
    using Infrastructure.Options;
    using List;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Projections.Legacy;
    using Query;
    using Swashbuckle.AspNetCore.Filters;
    using Sync;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("gebouwen")]
    [ApiExplorerSettings(GroupName = "Gebouwen")]
    public class BuildingController : ApiController
    {
        private readonly IMediator _mediator;

        public BuildingController(IMediator mediator)
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
        [ProducesResponseType(typeof(BuildingDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(BuildingNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(BuildingGoneResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var response =
                await _mediator.Send(
                    new GetRequest(persistentLocalId),
                    cancellationToken);

            return string.IsNullOrWhiteSpace(response.LastEventHash)
                ? Ok(response.BuildingDetailResponse)
                : new OkWithLastObservedPositionAsETagResult(response.BuildingDetailResponse, response.LastEventHash);
        }

        /// <summary>
        /// Vraag de referenties van een gebouw op.
        /// </summary>
        /// <param name="persistentLocalId"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">De referenties van het gebouw.</response>
        /// <response code="404">Als het gebouw niet gevonden kan worden.</response>
        /// <response code="410">Als het gebouw verwijderd werd.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{persistentLocalId}/referenties")]
        [ProducesResponseType(typeof(BuildingDetailReferencesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingReferencesResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(BuildingNotFoundResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(BuildingGoneResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> GetReferences(
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new GetReferencesRequest(persistentLocalId),
                cancellationToken);
            return Ok(response);
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
        [ProducesResponseType(typeof(BuildingListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingListResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> List(
            [FromServices] LegacyContext context,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var listResponse = await _mediator.Send(
                new ListRequest(
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
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Count(CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new CountRequest(
                    Request.ExtractFilteringRequest<BuildingFilter>(),
                    Request.ExtractSortingRequest()),
                cancellationToken);

            return Ok(response);
        }

        /// <summary>
        /// Vraag de koppeling tussen CRAB/GRB-gebouwen en GR-gebouwen
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van de CRAB/GRB-gebouwen gelukt is.</response>
        /// <response code="400">Als er geen parameters zijn opgegeven.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("crabgebouwen")]
        [ProducesResponseType(typeof(BuildingCrabMappingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingCrabMappingResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> CrabGebouwen(CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new CrabGebouwenRequest(
                    Request.ExtractFilteringRequest<BuildingCrabMappingFilter>(),
                    Request.ExtractSortingRequest()),
                cancellationToken);

            return response is null
                ? BadRequest("Filter is required")
                : Ok(response);
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
                Request.ExtractPaginationRequest()
            ), cancellationToken);

            return new ContentResult
            {
                Content = await BuildAtomFeed(response.LastFeedUpdate, response.PagedBuildings, responseOptions, configuration),
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
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
                    syndicationConfiguration["NextUri"]);
                if (nextUri is not null)
                {
                    await writer.Write(new SyndicationLink(nextUri, "next"));
                }

                foreach (var building in pagedBuildings.Items)
                {
                    await writer.WriteBuilding(
                        responseOptions,
                        formatter,
                        syndicationConfiguration["Category1"],
                        syndicationConfiguration["Category2"],
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
