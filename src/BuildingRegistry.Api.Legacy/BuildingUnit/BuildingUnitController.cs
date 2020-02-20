namespace BuildingRegistry.Api.Legacy.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Search.Filtering;
    using Be.Vlaanderen.Basisregisters.Api.Search.Pagination;
    using Be.Vlaanderen.Basisregisters.Api.Search.Sorting;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Converters;
    using Projections.Legacy;
    using Projections.Syndication;
    using Query;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Search;
    using Infrastructure;
    using Projections.Legacy.BuildingUnitDetail;
    using ValueObjects;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("gebouweenheden")]
    [ApiExplorerSettings(GroupName = "Gebouweenheden")]
    public class BuildingUnitController : ApiController
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
        [ProducesResponseType(typeof(BuildingUnitListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingUnitListResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> List(
            [FromServices] LegacyContext context,
            [FromServices] SyndicationContext syndicationContext,
            [FromServices] IOptions<ResponseOptions> responseOptions,
            CancellationToken cancellationToken = default)
        {
            var filtering = Request.ExtractFilteringRequest<BuildingUnitFilter>();
            var sorting = Request.ExtractSortingRequest();
            var pagination = Request.ExtractPaginationRequest();

            long Count(IQueryable<BuildingUnitDetailItem> items) => context.BuildingUnitDetailListCountView.Single().Count;

            var pagedBuildingUnits = new BuildingUnitListQuery(context, syndicationContext)
                .Fetch(
                    filtering,
                    sorting,
                    pagination,
                    items => -1);
                    //filtering.ShouldFilter ? null : (Func<IQueryable<BuildingUnitDetailItem>, long>)Count);

            Response.AddPagedQueryResultHeaders(pagedBuildingUnits);

            var units = await pagedBuildingUnits.Items
                .Select(a => new
                {
                    a.PersistentLocalId,
                    a.Version,
                })
                .ToListAsync(cancellationToken);

            var listResponse = new BuildingUnitListResponse
            {
                Gebouweenheden = units
                    .Select(x => new GebouweenheidCollectieItem(
                        x.PersistentLocalId.Value,
                        responseOptions.Value.GebouweenheidNaamruimte,
                        responseOptions.Value.GebouweenheidDetailUrl,
                        x.Version.ToBelgianDateTimeOffset()))
                    .ToList(),
                TotaalAantal = pagedBuildingUnits.PaginationInfo.TotalItems,
                Volgende = pagedBuildingUnits.PaginationInfo.BuildNextUri(responseOptions.Value.GebouweenheidVolgendeUrl)
            };

            return Ok(listResponse);
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
        [ProducesResponseType(typeof(BuildingUnitResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status410Gone)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(BuildingUnitResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(BuildingUnitNotFoundResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status410Gone, typeof(BuildingUnitGoneResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
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

            if (buildingUnit == null || !buildingUnit.IsComplete)
                throw new ApiException("Onbestaande gebouweenheid.", StatusCodes.Status404NotFound);

            var addressIds = buildingUnit.Addresses.Select(x => x.AddressId).ToList();
            var addressPersistentLocalIds = await syndicationContext
                .AddressPersistentLocalIds
                .Where(x => addressIds.Contains(x.AddressId))
                .Select(x => x.PersistentLocalId)
                .ToListAsync(cancellationToken);

            var response = new BuildingUnitResponse(
                buildingUnit.PersistentLocalId.Value,
                responseOptions.Value.GebouweenheidNaamruimte,
                buildingUnit.Version.ToBelgianDateTimeOffset(),
                GetBuildingUnitPoint(buildingUnit.Position),
                MapBuildingUnitGeometryMethod(buildingUnit.PositionMethod.Value),
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

        public static Point GetBuildingUnitPoint(byte[] point)
        {
            var geometry = WKBReaderFactory.Create().Read(point);
            return new Point
            {
                XmlPoint = new GmlPoint { Pos = $"{geometry.Coordinate.X.ToPointGeometryCoordinateValueFormat()} {geometry.Coordinate.Y.ToPointGeometryCoordinateValueFormat()}" },
                JsonPoint = new GeoJSONPoint { Coordinates = new[] { geometry.Coordinate.X, geometry.Coordinate.Y } }
            };
        }
    }
}
