namespace BuildingRegistry.Api.Oslo.BuildingUnit.V3.Detail
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.Gml;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class BuildingUnitOsloV3Response
    {
        /// <summary>
        /// De linked-data context van de gebouweenheid.
        /// </summary>
        [JsonProperty(PropertyName = "@context", Order = 0, Required = Required.DisallowNull)]
        public string Context { get; }

        /// <summary>
        /// Het linked-data type van de gebouweenheid.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order = 1, Required = Required.DisallowNull)]
        public string Type => "GebouweenheidEnvelop";

        /// <summary>
        /// De gebouweenheid data.
        /// </summary>
        [JsonProperty(PropertyName = "data", Order = 2, Required = Required.DisallowNull)]
        public BuildingUnitOsloV3ResponseData Data { get; set; }

        public BuildingUnitOsloV3Response(
            int persistentLocalId,
            string contextUrlUnitDetail,
            DateTimeOffset version,
            BuildingUnitPositionV3 buildingUnitPosition,
            GebouweenheidStatusValue status,
            GebouweenheidFunctieValue function,
            GebouweenheidIsDeelVan building,
            List<GebouweenheidToegekendAdres> addresses,
            bool hasDeviation)
        {
            Context = contextUrlUnitDetail;
            Data = new BuildingUnitOsloV3ResponseData(
                persistentLocalId,
                version,
                buildingUnitPosition,
                status,
                function,
                building,
                addresses,
                hasDeviation);
        }
    }

    public class BuildingUnitOsloV3ResponseData
    {
        /// <summary>
        /// Het linked-data type van de gebouweenheid.
        /// </summary>
        [JsonProperty(PropertyName = "@type", Order = 0, Required = Required.DisallowNull)]
        public string Type => "Gebouweenheid";

        /// <summary>
        /// De unieke en persistente identificator van de gebouweenheid (volgt de Vlaamse URI-standaard).
        /// </summary>
        [JsonProperty(PropertyName = "@id", Order = 1, Required = Required.DisallowNull)]
        public string Id { get; set; }

        /// <summary>
        /// De identificator van de gebouweenheid.
        /// </summary>
        [JsonProperty(PropertyName = "identificator", Order = 2, Required = Required.DisallowNull)]
        public GebouweenheidIdentificator Identificator { get; set; }

        /// <summary>
        /// De geometrie van het object in gml-formaat.
        /// </summary>
        [JsonProperty(PropertyName = "positie", Order = 3, Required = Required.DisallowNull)]
        public BuildingUnitPositionV3 BuildingUnitPosition { get; set; }

        /// <summary>
        /// De status van de gebouweenheid.
        /// </summary>
        [JsonProperty(PropertyName = "status", Order = 4, Required = Required.DisallowNull)]
        public GebouweenheidStatus Status { get; set; }

        /// <summary>
        /// De functie van de gebouweenheid in werkelijkheid (zoals waargenomen ter plaatse).
        /// </summary>
        [JsonProperty(PropertyName = "functie", Order = 5, Required = Required.DisallowNull)]
        public GebouweenheidFunctie Function { get; set; }

        /// <summary>
        /// building wherein the building unit resides
        /// </summary>
        [JsonProperty(PropertyName = "isDeelVan", Order = 6, Required = Required.DisallowNull)]
        public GebouweenheidIsDeelVan Building { get; set; }

        /// <summary>
        /// De aan de gebouweenheid gekoppelde adressen.
        /// </summary>
        [JsonProperty(PropertyName = "toegekendAdres", Order = 7, Required = Required.DisallowNull)]
        public List<GebouweenheidToegekendAdres> Addresses { get; set; }

        /// <summary>
        /// Wanneer de definitie van een gebouweenheid niet werd gevolgd en dus 'afwijkend' is.
        /// </summary>
        [JsonProperty(PropertyName = "afwijkingVastgesteld", Order = 8, Required = Required.DisallowNull)]
        public bool HasDeviation { get; set; }

        public BuildingUnitOsloV3ResponseData(
            int persistentLocalId,
            DateTimeOffset version,
            BuildingUnitPositionV3 buildingUnitPosition,
            GebouweenheidStatusValue status,
            GebouweenheidFunctieValue function,
            GebouweenheidIsDeelVan building,
            List<GebouweenheidToegekendAdres> addresses,
            bool hasDeviation)
        {
            Id = OsloNamespaces.Gebouweenheid.ToPuri(persistentLocalId.ToString());
            Identificator = new GebouweenheidIdentificator(persistentLocalId.ToString(), version);
            BuildingUnitPosition = buildingUnitPosition;
            Status = new GebouweenheidStatus(status);
            Function = new GebouweenheidFunctie(function);
            Building = building;
            Addresses = addresses.OrderBy(x => x.Id).ToList();
            HasDeviation = hasDeviation;
        }
    }

    public class BuildingUnitOsloResponseExamples : IExamplesProvider<BuildingUnitOsloV3Response>
    {
        private readonly ResponseOptionsV3 _responseOptions;

        public BuildingUnitOsloResponseExamples(IOptions<ResponseOptionsV3> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingUnitOsloV3Response GetExamples()
            => new BuildingUnitOsloV3Response
            (
                6,
                _responseOptions.ContextUrlUnitDetail,
                DateTimeOffset.Now.ToExampleOffset(),
                new BuildingUnitPositionV3(
                    [
                        new PointGeometrie("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>140252.76 198794.27</gml:pos></gml:Point>"),
                        new PointGeometrie("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>640249.09 698793.29</gml:pos></gml:Point>"),
                    ],
                    PositieGeometrieMethode.AangeduidDoorBeheerder),
                GebouweenheidStatusValue.Gerealiseerd,
                GebouweenheidFunctieValue.GemeenschappelijkDeel,
                new GebouweenheidIsDeelVan(OsloNamespaces.Gebouw.ToPuri("1"), new Uri(string.Format(_responseOptions.GebouwDetailUrl,"1"))),
                new List<GebouweenheidToegekendAdres>
                {
                    new GebouweenheidToegekendAdres(OsloNamespaces.Adres.ToPuri("1"), new Uri(string.Format(_responseOptions.AdresUrl,"1"))),
                    new GebouweenheidToegekendAdres(OsloNamespaces.Adres.ToPuri("7"), new Uri(string.Format(_responseOptions.AdresUrl,"7"))),
                    new GebouweenheidToegekendAdres(OsloNamespaces.Adres.ToPuri("10"),new Uri(string.Format(_responseOptions.AdresUrl,"10")))
                },
                false
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
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v3")
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
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext, "v3")
            };
    }
}
