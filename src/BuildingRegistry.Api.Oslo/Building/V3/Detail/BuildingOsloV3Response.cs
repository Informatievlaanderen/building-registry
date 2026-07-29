namespace BuildingRegistry.Api.Oslo.Building.V3.Detail
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gml;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using GebouwStatus = Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouw.GebouwStatus;

    public class BuildingOsloV3Response
    {
        /// <summary>
        /// De linked-data context van het gebouw.
        /// </summary>
        [JsonProperty(PropertyName = "@context", Order = 0, Required = Required.DisallowNull)]
        public string Context { get; }

        /// <summary>
        /// Het linked-data type van de gebouw envelop.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order = 1, Required = Required.DisallowNull)]
        public string Type => "GebouwEnvelop";

        /// <summary>
        /// De data van het gebouw.
        /// </summary>
        [JsonProperty(PropertyName = "data", Order = 2, Required = Required.DisallowNull)]
        public BuildingOsloV3ResponseData Data { get; set; }

        public BuildingOsloV3Response(
            int persistentLocalId,
            string contextUrlDetail,
            DateTimeOffset version,
            BuildingPolygonV3? buildingPolygon,
            GebouwStatusValue status,
            List<GebouwBestaatUitGebouweenheid> buildingUnits,
            List<GebouwLigtOpPerceel> parcels)
        {
            Context = contextUrlDetail;
            Data = new BuildingOsloV3ResponseData(persistentLocalId, version, buildingPolygon, status, buildingUnits, parcels);
        }
    }

    /// <summary>
    /// De data van het gebouw.
    /// </summary>
    public class BuildingOsloV3ResponseData
    {
        /// <summary>
        /// Het linked-data type van het gebouw.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order = 0, Required = Required.DisallowNull)]
        public string Type => "Gebouw";

        /// <summary>
        /// De unieke en persistente identificator van het gebouw (volgt de Vlaamse URI-standaard).
        /// </summary>
        [JsonProperty(PropertyName = "@id", Order = 1, Required = Required.DisallowNull)]
        public string Id { get; set; }

        /// <summary>
        /// De identificator van het gebouw.
        /// </summary>
        [JsonProperty(PropertyName = "identificator", Order = 2, Required = Required.DisallowNull)]
        public GebouwIdentificator Identificator { get; set; }

        /// <summary>
        /// de gebouwgeometrie (een simpele polygon met Lambert-72 en Lambert-08 (optioneel) coördinaten)
        /// </summary>
        [JsonProperty(PropertyName = "geometrie", Order = 3, Required = Required.Default, NullValueHandling = NullValueHandling.Include)]
        public BuildingPolygonV3? BuildingPolygon { get; set; }

        /// <summary>
        /// De fase in het leven van een gebouw.
        /// </summary>
        [JsonProperty(PropertyName = "status", Order = 4, Required = Required.DisallowNull)]
        public GebouwStatus Status { get; set; }

        /// <summary>
        /// De aan het gebouw gekoppelde gebouweenheden.
        /// </summary>
        [JsonProperty(PropertyName = "bestaatUit", Order = 5, Required = Required.DisallowNull)]
        public List<GebouwBestaatUitGebouweenheid> BuildingUnits { get; set; }

        /// <summary>
        /// De aan het gebouw gekoppelde percelen.
        /// </summary>
        [JsonProperty(PropertyName = "ligtOp", Order = 6, Required = Required.DisallowNull)]
        public List<GebouwLigtOpPerceel> Parcels { get; set; }

        public BuildingOsloV3ResponseData(int persistentLocalId,
            DateTimeOffset version,
            BuildingPolygonV3? buildingPolygon,
            GebouwStatusValue status,
            List<GebouwBestaatUitGebouweenheid> buildingUnits,
            List<GebouwLigtOpPerceel> parcels)
        {
            Id = OsloNamespaces.Gebouw.ToPuri(persistentLocalId.ToString());
            Identificator = new GebouwIdentificator(persistentLocalId.ToString(), version);
            BuildingPolygon = buildingPolygon;
            Status = new GebouwStatus(status);
            BuildingUnits = buildingUnits;
            Parcels = parcels;
        }
    }

    public class BuildingOsloResponseExamples : IExamplesProvider<BuildingOsloV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public BuildingOsloResponseExamples(IOptions<ResponseOptionsV3> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingOsloV3Response GetExamples()
        {
            var buildingPolygon = new BuildingPolygonV3(
                [
                    new PolygonGeometrie("<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>101673.0 193520.0 101673.0 193585.0 101732.0 193585.0 101673.0 193585.0 101673.0 193520.0</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>"),
                    new PolygonGeometrie("<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>601670.3 693514.5 601670.3 693579.5 601729.3 693579.5 601670.3 693579.5 601670.3 693514.5</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>")
                ],
                GebouwGeometrieMethode.IngemetenGRB);

            return new BuildingOsloV3Response(
                6,
                _responseOptions.ContextUrlDetail,
                DateTimeOffset.Now.ToExampleOffset(),
                buildingPolygon,
                GebouwStatusValue.Gerealiseerd,
                new List<GebouwBestaatUitGebouweenheid>
                {
                    new GebouwBestaatUitGebouweenheid(OsloNamespaces.Gebouweenheid.ToPuri("1"), GebouweenheidStatusValue.Gerealiseerd, new Uri(string.Format(_responseOptions.GebouweenheidDetailUrl,"1"))),
                    new GebouwBestaatUitGebouweenheid(OsloNamespaces.Gebouweenheid.ToPuri("2"), GebouweenheidStatusValue.Gerealiseerd, new Uri(string.Format(_responseOptions.GebouweenheidDetailUrl,"2")))
                },
                new List<GebouwLigtOpPerceel>
                {
                    new GebouwLigtOpPerceel(OsloNamespaces.Perceel.ToPuri("11001B0008-00G002"), new Uri(string.Format(_responseOptions.PerceelUrl,"11001B0008-00G002"))),
                    new GebouwLigtOpPerceel(OsloNamespaces.Perceel.ToPuri("11001B0008-00G003"), new Uri(string.Format(_responseOptions.PerceelUrl,"11001B0008-00G003")))
                });
        }
    }

    public class BuildingNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public BuildingNotFoundResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples()
            => new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:building:not-found",
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Onbestaand gebouw.",
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v3")
            };
    }

    public class BuildingGoneResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public BuildingGoneResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples()
            => new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:building:gone",
                HttpStatus = StatusCodes.Status410Gone,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Verwijderd gebouw.",
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v3")
            };
    }
}
