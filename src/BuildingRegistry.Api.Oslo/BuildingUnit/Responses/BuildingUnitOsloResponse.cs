namespace BuildingRegistry.Api.Oslo.BuildingUnit.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Abstractions.Infrastructure.Options;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "GebouweenheidDetail", Namespace = "")]
    public class BuildingUnitOsloResponse
    {
        /// <summary>
        /// De linked-data context van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Context { get; }

        /// <summary>
        /// Het linked-data type van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "@type", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "Gebouweenheid";

        /// <summary>
        /// De identificator van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouweenheidIdentificator Identificator { get; set; }

        /// <summary>
        /// De geometrie van het object in gml-formaat.
        /// </summary>
        [DataMember(Name = "GebouweenheidPositie", Order = 3)]
        [JsonProperty(Required = Required.DisallowNull)]
        public BuildingUnitPosition BuildingUnitPosition { get; set; }

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

        public BuildingUnitOsloResponse(
            int persistentLocalId,
            string naamruimte,
            string contextUrlUnitDetail,
            DateTimeOffset version,
            BuildingUnitPosition buildingUnitPosition,
            GebouweenheidStatus status,
            GebouweenheidFunctie? function,
            GebouweenheidDetailGebouw building,
            List<GebouweenheidDetailAdres> addresses)
        {
            Context = contextUrlUnitDetail;
            Identificator = new GebouweenheidIdentificator(naamruimte, persistentLocalId.ToString(), version);
            BuildingUnitPosition = buildingUnitPosition;
            Status = status;
            Function = function;
            Building = building;
            Addresses = addresses.OrderBy(x => x.ObjectId).ToList();
        }
    }

    public class BuildingUnitOsloResponseExamples : IExamplesProvider<BuildingUnitOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingUnitOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingUnitOsloResponse GetExamples()
            => new BuildingUnitOsloResponse
            (
                6,
                _responseOptions.GebouweenheidNaamruimte,
                _responseOptions.ContextUrlUnitDetail,
                DateTimeOffset.Now.ToExampleOffset(),
                new BuildingUnitPosition(
                    new GmlJsonPoint("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>140252.76 198794.27</gml:pos></gml:Point>"),
                    PositieGeometrieMethode.AangeduidDoorBeheerder),
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

    public class BuildingUnitNotFoundOsloResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public BuildingUnitNotFoundOsloResponseExamples(
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

    public class BuildingUnitGoneOsloResponseExamples : IExamplesProvider<ProblemDetails>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ProblemDetailsHelper _problemDetailsHelper;

        public BuildingUnitGoneOsloResponseExamples(
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
