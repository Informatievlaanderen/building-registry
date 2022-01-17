namespace BuildingRegistry.Api.Oslo.BuildingUnit
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Infrastructure;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.Geometries;
    using Projections.Legacy;
    using Projections.Syndication;
    using Query;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using ValueObjects;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("2.0")]
    [AdvertiseApiVersions("2.0")]
    [ApiRoute("gebouweenheden")]
    [ApiExplorerSettings(GroupName = "Gebouweenheden")]
    public class BuildingUnitOsloController : ApiController
    {
        /// <summary>
        /// Vraag een lijst met actieve gebouweenheden op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="responseOptions"></param>
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
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<BuildingUnitFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            var pagedBuildingUnits = new BuildingUnitListOsloQuery(context, syndicationContext)
                .Fetch(filtering, sorting, pagination);

            Response.AddPagedQueryResultHeaders(pagedBuildingUnits);

            var units = await pagedBuildingUnits.Items
                .Select(a => new
                {
                    a.PersistentLocalId,
                    a.Version,
                    a.Status,
                })
                .ToListAsync(cancellationToken);

            var listResponse = new BuildingUnitListOsloResponse
            {
                Gebouweenheden = units
                    .Select(x => new GebouweenheidCollectieItemOslo(
                        x.PersistentLocalId.Value,
                        responseOptions.Value.GebouweenheidNaamruimte,
                        responseOptions.Value.GebouweenheidDetailUrl,
                        MapBuildingUnitStatus(x.Status.Value),
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                Volgende = pagedBuildingUnits
                    .PaginationInfo
                    .BuildNextUri(responseOptions.Value.GebouweenheidVolgendeUrl)
            };

            return Ok(listResponse);
        }

        /// <summary>
        /// Vraag het totaal aantal actieve gebouweenheden op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
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
            [FromServices] SyndicationContext syndicationContext,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<BuildingUnitFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = new NoPaginationRequest();

            return Ok(
                new TotaalAantalResponse
                {
                    Aantal = filtering.ShouldFilter
                        ? await new BuildingUnitListOsloQuery(context, syndicationContext)
                            .Fetch(filtering, sorting, pagination)
                            .Items
                            .CountAsync(cancellationToken)
                        : Convert.ToInt32(context
                            .BuildingUnitDetailListCountView
                            .First()
                            .Count)
                });
        }

        /// <summary>
        /// Vraag een gebouweenheid op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="syndicationContext"></param>
        /// <param name="responseOptions"></param>
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
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(BuildingUnitNotFoundOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(BuildingUnitGoneOsloResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            [FromRoute] int persistentLocalId,
            CancellationToken cancellationToken = default)
        {
            var buildingUnit = await context
                .BuildingUnitDetails
                .Include(x => x.Addresses)
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.PersistentLocalId == persistentLocalId, cancellationToken);

            if (buildingUnit != null && buildingUnit.IsRemoved)
                throw new ApiException("Gebouweenheid werd verwijderd.", StatusCodes.Status410Gone);

            if (buildingUnit == null || !buildingUnit.IsComplete || !buildingUnit.IsBuildingComplete)
                throw new ApiException("Onbestaande gebouweenheid.", StatusCodes.Status404NotFound);

            var addressIds = buildingUnit.Addresses.Select(x => x.AddressId).ToList();
            var addressPersistentLocalIds = await syndicationContext
                .AddressPersistentLocalIds
                .Where(x => addressIds.Contains(x.AddressId))
                .Select(x => x.PersistentLocalId)
                .ToListAsync(cancellationToken);

            var response = new BuildingUnitOsloResponse(
                buildingUnit.PersistentLocalId.Value,
                responseOptions.Value.GebouweenheidNaamruimte,
                buildingUnit.Version.ToBelgianDateTimeOffset(),
                GetBuildingUnitPoint(buildingUnit.Position, buildingUnit.PositionMethod.Value),
                MapBuildingUnitStatus(buildingUnit.Status.Value),
                MapBuildingUnitFunction(buildingUnit.Function),
                new GebouweenheidDetailGebouw(buildingUnit.BuildingPersistentLocalId.Value.ToString(), string.Format(responseOptions.Value.GebouwDetailUrl, buildingUnit.BuildingPersistentLocalId.Value)),
                addressPersistentLocalIds.Select(id => new GebouweenheidDetailAdres(id, string.Format(responseOptions.Value.AdresUrl, id))).ToList());

            return Ok(response);
        }

        private static PositieGeometrieMethode MapBuildingUnitGeometryMethod(
            BuildingUnitPositionGeometryMethod geometryMethod)
        {
            if (BuildingUnitPositionGeometryMethod.AppointedByAdministrator == geometryMethod)
                return PositieGeometrieMethode.AangeduidDoorBeheerder;

            if (BuildingUnitPositionGeometryMethod.DerivedFromObject == geometryMethod)
                return PositieGeometrieMethode.AfgeleidVanObject;

            throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, null);
        }

        private static GebouweenheidStatus MapBuildingUnitStatus(
            BuildingUnitStatus status)
        {
            if (BuildingUnitStatus.Planned == status)
                return GebouweenheidStatus.Gepland;

            if (BuildingUnitStatus.NotRealized == status)
                return GebouweenheidStatus.NietGerealiseerd;

            if (BuildingUnitStatus.Realized == status)
                return GebouweenheidStatus.Gerealiseerd;

            if (BuildingUnitStatus.Retired == status)
                return GebouweenheidStatus.Gehistoreerd;

            throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        private static GebouweenheidFunctie? MapBuildingUnitFunction(
            BuildingUnitFunction? function)
        {
            if (function == null)
                return null;

            if (BuildingUnitFunction.Common == function)
                return GebouweenheidFunctie.GemeenschappelijkDeel;

            if (BuildingUnitFunction.Unknown == function)
                return GebouweenheidFunctie.NietGekend;

            throw new ArgumentOutOfRangeException(nameof(function), function, null);
        }

        private static Responses.BuildingUnitPosition GetBuildingUnitPoint(byte[] point, BuildingUnitPositionGeometryMethod geometryMethod)
        {
            var geometry = WKBReaderFactory.Create().Read(point);
            var gml = GetGml(geometry);
            return new Responses.BuildingUnitPosition(new GmlJsonPoint(gml), MapBuildingUnitGeometryMethod(geometryMethod));
        }

        private static string GetGml(Geometry geometry)
        {
            StringBuilder builder = new();
            XmlWriterSettings settings = new() { Indent = false, OmitXmlDeclaration = true };
            using (XmlWriter xmlwriter = XmlWriter.Create(builder, settings))
            {
                xmlwriter.WriteStartElement("gml", "Point", "http://www.opengis.net/gml/3.2");
                xmlwriter.WriteAttributeString("srsName", "https://www.opengis.net/def/crs/EPSG/0/31370");
                Write(geometry.Coordinate, xmlwriter);
                xmlwriter.WriteEndElement();
            }
            return builder.ToString();
        }

        private static void Write(Coordinate coordinate, XmlWriter writer)
        {
            writer.WriteStartElement("gml", "pos", "http://www.opengis.net/gml/3.2");
            writer.WriteValue(string.Format(NetTopologySuite.Utilities.Global.GetNfi(), "{0} {1}", coordinate.X.ToPointGeometryCoordinateValueFormat(),
                coordinate.Y.ToPointGeometryCoordinateValueFormat()));
            writer.WriteEndElement();
        }
    }
}
