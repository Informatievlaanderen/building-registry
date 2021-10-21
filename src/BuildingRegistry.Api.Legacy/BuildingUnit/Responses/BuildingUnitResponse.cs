namespace BuildingRegistry.Api.Legacy.BuildingUnit.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    [DataContract(Name = "GebouweenheidDetail", Namespace = "")]
    public class BuildingUnitResponse
    {
        /// <summary>
        /// De identificator van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouweenheidIdentificator Identificator { get; set; }

        /// <summary>
        /// De puntgeometrie van de gebouweenheid in gml- of geoJSON-formaat, afhankelijk van het content-type.
        /// </summary>
        [DataMember(Name = "GeometriePunt", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public Point Geometry { get; set; }

        /// <summary>
        /// De geometriemethode van de gebouweenheidpositie. 
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public PositieGeometrieMethode GeometryMethod { get; set; }

        /// <summary>
        /// De status van de gebouweenheid. 
        /// </summary>
        [DataMember(Name = "GebouweenheidStatus", Order = 4)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouweenheidStatus Status { get; set; }

        /// <summary>
        /// the function of the building unit in reality (as observed on site)
        /// </summary>
        [DataMember(Name = "Functie", Order = 5)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouweenheidFunctie? Function { get; set; }

        /// <summary>
        /// building wherein the building unit resides
        /// </summary>
        [DataMember(Name = "Gebouw", Order = 6)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouweenheidDetailGebouw Building { get; set; }

        /// <summary>
        /// De aan de gebouweenheid gekoppelde adressen.
        /// </summary>
        [DataMember(Name = "Adressen", Order = 7)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<GebouweenheidDetailAdres> Addresses { get; set; }

        public BuildingUnitResponse(
            int persistentLocalId,
            string naamruimte,
            DateTimeOffset version,
            Point geometry,
            PositieGeometrieMethode geometryMethod,
            GebouweenheidStatus status,
            GebouweenheidFunctie? function,
            GebouweenheidDetailGebouw building,
            List<GebouweenheidDetailAdres> addresses)
        {
            Identificator = new GebouweenheidIdentificator(naamruimte, persistentLocalId.ToString(), version);
            Geometry = geometry;
            GeometryMethod = geometryMethod;
            Status = status;
            Function = function;
            Building = building;
            Addresses = addresses.OrderBy(x => x.ObjectId).ToList();
        }
    }

    public class BuildingUnitResponseExamples : IExamplesProvider<BuildingUnitResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingUnitResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingUnitResponse GetExamples()
            => new BuildingUnitResponse
            (
                6,
                _responseOptions.GebouweenheidNaamruimte,
                DateTimeOffset.Now.ToExampleOffset(),
                new Point
                {
                    JsonPoint = new GeoJSONPoint
                    {
                        Type = "point",
                        Coordinates = new[] { 140252.76, 198794.27 }
                    },
                    XmlPoint = new GmlPoint
                    {
                        Pos = "140252.76 198794.27"
                    }
                },
                PositieGeometrieMethode.AangeduidDoorBeheerder,
                GebouweenheidStatus.Gerealiseerd,
                GebouweenheidFunctie.GemeenschappelijkDeel,
                new GebouweenheidDetailGebouw("1", string.Format(_responseOptions.GebouwDetailUrl,"1")),
                new List<GebouweenheidDetailAdres>
                {
                    new GebouweenheidDetailAdres("1", string.Format(_responseOptions.AdresUrl,"1")),
                    new GebouweenheidDetailAdres("7", string.Format(_responseOptions.AdresUrl,"7")),
                    new GebouweenheidDetailAdres("10",string.Format(_responseOptions.AdresUrl,"10"))
                }
            );
    }

    public class BuildingUnitNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public BuildingUnitNotFoundResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples()
            => new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:buildingunit:not-found",
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Onbestaande gebouweenheid.",
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext)
            };
    }

    public class BuildingUnitGoneResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public BuildingUnitGoneResponseExamples(
            IHttpContextAccessor httpContextAccessor,
            ProblemDetailsHelper problemDetailsHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _problemDetailsHelper = problemDetailsHelper;
        }

        public ProblemDetails GetExamples()
            => new ProblemDetails
            {
                ProblemTypeUri = "urn:be.vlaanderen.basisregisters.api:buildingunit:gone",
                HttpStatus = StatusCodes.Status410Gone,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Verwijderde gebouweenheid.",
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext)
            };
    }
}
