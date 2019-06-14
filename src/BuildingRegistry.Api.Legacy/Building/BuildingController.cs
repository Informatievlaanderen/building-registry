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
    using GeoAPI.Geometries;
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
        /// <param name="gebouwId"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als het gebouw gevonden is.</response>
        /// <response code="404">Als het gebouw niet gevonden kan worden.</response>
        /// <response code="410">Als het gebouw verwijderd werd.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{gebouwId}")]
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
            [FromRoute] int gebouwId,
            CancellationToken cancellationToken = default)
        {
            var building = await context
                .BuildingDetails
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.OsloId == gebouwId, cancellationToken);

            if (building == null || !building.IsComplete)
                throw new ApiException("Onbestaand gebouw.", StatusCodes.Status404NotFound);

            if (building.IsRemoved)
                throw new ApiException("Gebouw werd verwijderd.", StatusCodes.Status410Gone);

            //TODO: improvement getting buildingunits and parcels in parallel.
            var buildingUnits = await context
                .BuildingUnitDetails
                .Where(x => x.BuildingId == building.BuildingId)
                .Select(x => x.OsloId)
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
                building.OsloId.Value,
                responseOptions.Value.GebouwNaamruimte,
                building.Version.ToBelgianDateTimeOffset(),
                GetBuildingPolygon(building.Geometry),
                MapGeometryMethod(building.GeometryMethod.Value),
                MapBuildingStatus(building.Status.Value),
                buildingUnits.Select(x => new GebouwDetailGebouweenheid(x.ToString(), string.Format(responseOptions.Value.GebouweenheidDetailUrl, x))).ToList(),
                caPaKeys.Select(x => new GebouwDetailPerceel(x, string.Format(responseOptions.Value.PerceelUrl, x))).ToList());

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
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingListResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> List(
            [FromServices] LegacyContext context,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var pagedBuildings = new BuildingListQuery(context)
                .Fetch(new FilteringHeader<BuildingFilter>(new BuildingFilter()),
                    sorting,
                    pagination);

            Response.AddPaginationResponse(pagedBuildings.PaginationInfo);
            Response.AddSortingResponse(sorting.SortBy, sorting.SortOrder);

            var buildings = await pagedBuildings.Items
                .Select(a => new
                {
                    a.OsloId,
                    a.Version
                })
                .ToListAsync(cancellationToken);

            var listResponse = new BuildingListResponse
            {
                Gebouwen = buildings
                    .Select(x => new GebouwCollectieItem(
                        x.OsloId.Value,
                        responseOptions.Value.GebouwNaamruimte,
                        responseOptions.Value.GebouwDetailUrl,
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                TotaalAantal = pagedBuildings.PaginationInfo.TotalItems,
                Volgende = BuildVolgendeUri(pagedBuildings.PaginationInfo, responseOptions.Value.GebouwVolgendeUrl)
            };

            return Ok(listResponse);
        }

        internal static Polygon GetBuildingPolygon(IPolygon geometry)
        {
            return new Polygon
            {
                XmlPolygon = MapGmlPolygon(geometry),
                JsonPolygon = MapToGeoJsonPolygon(geometry)
            };
        }

        private static GeoJSONPolygon MapToGeoJsonPolygon(IPolygon polygon)
        {
            var rings = polygon.InteriorRings.ToList();
            rings.Insert(0, polygon.ExteriorRing); //insert exterior ring as first item

            double[][][] output = new double[rings.Count][][];
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

        private static GmlPolygon MapGmlPolygon(IPolygon polygon)
        {
            var gmlPolygon = new GmlPolygon { Interior = new List<RingProperty>() };

            gmlPolygon.Exterior = GetGmlRing(polygon.ExteriorRing as ILinearRing);

            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                gmlPolygon.Interior.Add(GetGmlRing(polygon.InteriorRings[i] as ILinearRing));
            }

            return gmlPolygon;
        }

        private static RingProperty GetGmlRing(ILinearRing ring)
        {
            var posListBuilder = new StringBuilder();
            foreach (var coordinate in ring.Coordinates)
            {
                posListBuilder.Append($"{coordinate.X} {coordinate.Y} ");
            }

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

        internal static Uri BuildVolgendeUri(PaginationInfo paginationInfo, string volgendeUrlBase)
        {
            var offset = paginationInfo.Offset;
            var limit = paginationInfo.Limit;

            return offset + limit < paginationInfo.TotalItems
                ? new Uri(string.Format(volgendeUrlBase, offset + limit, limit))
                : null;
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

            var pagedBuildings = new BuildingSyndicationQuery(
                context,
                filtering.Filter?.ContainEvent ?? false,
                filtering.Filter?.ContainObject ?? false)
                .Fetch(filtering, sorting, pagination);

            Response.AddPaginationResponse(pagedBuildings.PaginationInfo);
            Response.AddSortingResponse(sorting.SortBy, sorting.SortOrder);

            return new ContentResult
            {
                Content = await BuildAtomFeed(pagedBuildings, responseOptions, configuration),
                ContentType = MediaTypeNames.Text.Xml,
                StatusCode = StatusCodes.Status200OK
            };
        }

        private static async Task<string> BuildAtomFeed(
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

                await writer.WriteDefaultMetadata(
                    syndicationConfiguration["Id"],
                    syndicationConfiguration["Title"],
                    Assembly.GetEntryAssembly().GetName().Version.ToString(),
                    new Uri(syndicationConfiguration["Self"]),
                    syndicationConfiguration.GetSection("Related").GetChildren().Select(c => c.Value).ToArray());

                var nextUri = BuildVolgendeUri(pagedBuildings.PaginationInfo, syndicationConfiguration["NextUri"]);
                if(nextUri != null)
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
    }
}
