namespace BuildingRegistry.Api.Legacy.Building
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.Api.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Infrastructure.Grb;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Newtonsoft.Json.Converters;
    using Projections.Legacy;
    using Projections.Syndication;
    using Query;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mime;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure;
    using ValueObjects;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("gebouwen")]
    [ApiExplorerSettings(GroupName = "Gebouwen")]
    public class BuildingController : ApiController
    {
        /// <summary>
        /// Vraag een gebouw op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="responseOptions"></param>
        /// <param name="grbBuildingParcel"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als het gebouw gevonden is.</response>
        /// <response code="404">Als het gebouw niet gevonden kan worden.</response>
        /// <response code="410">Als het gebouw verwijderd werd.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{persistentLocalId}")]
        [ProducesResponseType(typeof(BuildingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(BuildingNotFoundResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(BuildingGoneResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromServices] IGrbBuildingParcel grbBuildingParcel,
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var building = await context
                .BuildingDetails
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == persistentLocalId, cancellationToken);

            if (building != null && building.IsRemoved)
                throw new ApiException("Gebouw werd verwijderd.", StatusCodes.Status410Gone);

            if (building == null || !building.IsComplete)
                throw new ApiException("Onbestaand gebouw.", StatusCodes.Status404NotFound);

            //TODO: improvement getting buildingunits and parcels in parallel.
            var buildingUnits = await context
                .BuildingUnitDetails
                .Where(x => x.BuildingId == building.BuildingId)
                .Where(x => x.IsComplete && !x.IsRemoved)
                .Select(x => x.PersistentLocalId)
                .ToListAsync(cancellationToken);

            var parcels = grbBuildingParcel
                .GetUnderlyingParcels(building.Geometry)
                .Select(s => CaPaKey.CreateFrom(s).VbrCaPaKey)
                .Distinct();

            var caPaKeys = await syndicationContext
                .BuildingParcelLatestItems
                .Where(x => !x.IsRemoved &&
                            x.IsComplete &&
                            parcels.Contains(x.CaPaKey))
                .Select(x => x.CaPaKey)
                .ToListAsync(cancellationToken);

            var response = new BuildingResponse(
                building.PersistentLocalId.Value,
                responseOptions.Value.GebouwNaamruimte,
                building.Version.ToBelgianDateTimeOffset(),
                GetBuildingPolygon(building.Geometry),
                MapGeometryMethod(building.GeometryMethod.Value),
                MapBuildingStatus(building.Status.Value),
                buildingUnits.OrderBy(x => x.Value).Select(x => new GebouwDetailGebouweenheid(x.ToString(), string.Format(responseOptions.Value.GebouweenheidDetailUrl, x))).ToList(),
                caPaKeys.Select(x => new GebouwDetailPerceel(x, string.Format(responseOptions.Value.PerceelUrl, x))).ToList());

            return Ok(response);
        }

        /// <summary>
        /// Vraag de referenties van een gebouw op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="persistentLocalId"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">De referenties van het gebouw.</response>
        /// <response code="404">Als het gebouw niet gevonden kan worden.</response>
        /// <response code="410">Als het gebouw verwijderd werd.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{persistentLocalId}/referenties")]
        [ProducesResponseType(typeof(BuildingReferencesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingReferencesResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(BuildingNotFoundResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(BuildingGoneResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> GetReferences(
            [FromServices] LegacyContext context,
            [FromRoute] int persistentLocalId,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var building = await context
                .BuildingDetails
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == persistentLocalId, cancellationToken);

            if (building != null && building.IsRemoved)
                throw new ApiException("Gebouw werd verwijderd.", StatusCodes.Status410Gone);

            if (building == null || !building.IsComplete)
                throw new ApiException("Onbestaand gebouw.", StatusCodes.Status404NotFound);

            var crabMappings = await context.BuildingPersistentIdCrabIdMappings.FindAsync(new object[] { building.BuildingId }, cancellationToken);
            var crabReferences =
                crabMappings.CrabTerrainObjectId.HasValue && !string.IsNullOrEmpty(crabMappings.CrabIdentifierTerrainObject)
                ? new CrabReferences(crabMappings.CrabTerrainObjectId.Value, crabMappings.CrabIdentifierTerrainObject)
                : null;

            return Ok(new BuildingReferencesResponse(
                building.PersistentLocalId.Value,
                responseOptions.Value.GebouwNaamruimte,
                building.Version.ToBelgianDateTimeOffset(),
                crabReferences));
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
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingListResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> List(
            [FromServices] LegacyContext context,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<BuildingFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var pagedBuildings = new BuildingListQuery(context)
                .Fetch(filtering, sorting, pagination);

            Response.AddPagedQueryResultHeaders(pagedBuildings);

            var buildings = await pagedBuildings.Items
                .Select(a => new
                {
                    a.PersistentLocalId,
                    a.Version,
                    a.Status
                })
                .ToListAsync(cancellationToken);

            var listResponse = new BuildingListResponse
            {
                Gebouwen = buildings
                    .Select(x => new GebouwCollectieItem(
                        x.PersistentLocalId.Value,
                        responseOptions.Value.GebouwNaamruimte,
                        responseOptions.Value.GebouwDetailUrl,
                        MapBuildingStatus(x.Status.Value),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildings.PaginationInfo.BuildNextUri(responseOptions.Value.GebouwVolgendeUrl)
            };

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
        [ProducesResponseType(typeof(TotaalAantalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TotalCountResponseExample), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Count(
            [FromServices] LegacyContext context,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<BuildingFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            return Ok(
                new TotaalAantalResponse
                {
                    Aantal = filtering.ShouldFilter
                        ? await new BuildingListQuery(context)
                            .Fetch(filtering, sorting, pagination)
                            .Items
                            .CountAsync(cancellationToken)
                        : Convert.ToInt32(context
                            .BuildingDetailListCountView
                            .First()
                            .Count)
                });
        }

        /// <summary>
        /// Vraag de koppeling tussen CRAB/GRB-gebouwen en GR-gebouwen
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van de CRAB/GRB-gebouwen gelukt is.</response>
        /// <response code="400">Als er geen parameters zijn opgegeven.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("crabgebouwen")]
        [ProducesResponseType(typeof(BuildingCrabMappingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingCrabMappingResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> CrabGebouwen(
            [FromServices] LegacyContext context,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<BuildingCrabMappingFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            if (filtering.Filter.TerrainObjectId == null && string.IsNullOrEmpty(filtering.Filter.IdentifierTerrainObject))
                return BadRequest("Filter is required");

            var query = new BuildingCrabMappingQuery(context).Fetch(filtering, sorting, pagination);

            return Ok(new BuildingCrabMappingResponse
            {
                CrabGebouwen = query
                    .Items
                    .Select(x => new BuildingCrabMappingItem(x.PersistentLocalId.Value, x.CrabTerrainObjectId.Value, x.CrabIdentifierTerrainObject))
                    .ToList()
            });
        }

        internal static Polygon GetBuildingPolygon(byte[] polygon)
        {
            var geometry = WKBReaderFactory.Create().Read(polygon) as NetTopologySuite.Geometries.Polygon;

            if (geometry == null) //some buildings have multi polygons (imported) which are incorrect.
                return null;

            return new Polygon
            {
                XmlPolygon = MapGmlPolygon(geometry),
                JsonPolygon = MapToGeoJsonPolygon(geometry)
            };
        }

        private static GeoJSONPolygon MapToGeoJsonPolygon(NetTopologySuite.Geometries.Polygon polygon)
        {
            var rings = polygon.InteriorRings.ToList();
            rings.Insert(0, polygon.ExteriorRing); //insert exterior ring as first item

            var output = new double[rings.Count][][];
            for (var i = 0; i < rings.Count; i++)
            {
                output[i] = new double[rings[i].Coordinates.Length][];

                for (int j = 0; j < rings[i].Coordinates.Length; j++)
                {
                    output[i][j] = new double[2];
                    output[i][j][0] = rings[i].Coordinates[j].X;
                    output[i][j][1] = rings[i].Coordinates[j].Y;
                }
            }

            return new GeoJSONPolygon { Coordinates = output };
        }

        private static GmlPolygon MapGmlPolygon(NetTopologySuite.Geometries.Polygon polygon)
        {
            var gmlPolygon = new GmlPolygon
            {
                Exterior = GetGmlRing(polygon.ExteriorRing as NetTopologySuite.Geometries.LinearRing)
            };

            if (polygon.NumInteriorRings > 0)
                gmlPolygon.Interior = new List<RingProperty>();

            for (var i = 0; i < polygon.NumInteriorRings; i++)
                gmlPolygon.Interior.Add(GetGmlRing(polygon.InteriorRings[i] as NetTopologySuite.Geometries.LinearRing));

            return gmlPolygon;
        }

        private static RingProperty GetGmlRing(NetTopologySuite.Geometries.LinearRing ring)
        {
            var posListBuilder = new StringBuilder();
            foreach (var coordinate in ring.Coordinates)
                posListBuilder.Append($"{coordinate.X.ToPolygonGeometryCoordinateValueFormat()} {coordinate.Y.ToPolygonGeometryCoordinateValueFormat()} ");

            //remove last space
            posListBuilder.Length--;

            var gmlRing = new RingProperty { LinearRing = new LinearRing { PosList = posListBuilder.ToString() } };
            return gmlRing;
        }

        private static GeometrieMethode MapGeometryMethod(BuildingGeometryMethod geometryMethod)
        {
            switch (geometryMethod)
            {
                case BuildingGeometryMethod.Outlined:
                    return GeometrieMethode.Ingeschetst;

                case BuildingGeometryMethod.MeasuredByGrb:
                    return GeometrieMethode.IngemetenGRB;

                default:
                    throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
            }
        }

        private static GebouwStatus MapBuildingStatus(BuildingStatus status)
        {
            switch (status)
            {
                case BuildingStatus.Planned:
                    return GebouwStatus.Gepland;

                case BuildingStatus.UnderConstruction:
                    return GebouwStatus.InAanbouw;

                case BuildingStatus.Realized:
                    return GebouwStatus.Gerealiseerd;

                case BuildingStatus.Retired:
                    return GebouwStatus.Gehistoreerd;

                case BuildingStatus.NotRealized:
                    return GebouwStatus.NietGerealiseerd;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        /// <summary>
        /// Vraag een lijst met wijzigingen van gebouwen op.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="context"></param>
        /// <param name="responseOptions"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("sync")]
        [Produces("text/xml")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingSyndicationResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Sync(
            [FromServices] IConfiguration configuration,
            [FromServices] LegacyContext context,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<BuildingSyndicationFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var lastFeedUpdate = await context
                .BuildingSyndication
                .AsNoTracking()
                .OrderByDescending(item => item.Position)
                .Select(item => item.SyndicationItemCreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (lastFeedUpdate == default)
                lastFeedUpdate = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var pagedBuildings = new BuildingSyndicationQuery(
                context,
                filtering.Filter?.Embed)
                .Fetch(filtering, sorting, pagination);

            return new ContentResult
            {
                Content = await BuildAtomFeed(lastFeedUpdate, pagedBuildings, responseOptions, configuration),
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

            using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Async = true, Indent = true, Encoding = sw.Encoding }))
            {
                var formatter = new AtomFormatter(null, xmlWriter.Settings) { UseCDATA = true };
                var writer = new AtomFeedWriter(xmlWriter, null, formatter);
                var syndicationConfiguration = configuration.GetSection("Syndication");
                var atomConfiguration = AtomFeedConfigurationBuilder.CreateFrom(syndicationConfiguration, lastUpdate);

                await writer.WriteDefaultMetadata(atomConfiguration);

                var buildings = pagedBuildings.Items.ToList();

                var nextFrom = buildings.Any()
                    ? buildings.Max(x => x.Position) + 1
                    : (long?)null;

                var nextUri = BuildNextSyncUri(pagedBuildings.PaginationInfo.Limit, nextFrom, syndicationConfiguration["NextUri"]);
                if (nextUri != null)
                    await writer.Write(new SyndicationLink(nextUri, "next"));

                foreach (var building in pagedBuildings.Items)
                    await writer.WriteBuilding(
                        responseOptions,
                        formatter,
                        syndicationConfiguration["Category1"],
                        syndicationConfiguration["Category2"],
                        building);

                xmlWriter.Flush();
            }

            return sw.ToString();
        }

        private static Uri BuildNextSyncUri(int limit, long? from, string nextUrlBase)
        {
            return from.HasValue
                ? new Uri(string.Format(nextUrlBase, from, limit))
                : null;
        }
    }
}
