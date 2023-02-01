namespace BuildingRegistry.Api.Oslo.Building.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "GebouwDetail", Namespace = "")]
    public class BuildingOsloResponse
    {
        /// <summary>
        /// De linked-data context van het gebouw.
        /// </summary>
        [DataMember(Name = "@context", Order = 0)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Context { get; }

        /// <summary>
        /// Het linked-data type van het gebouw.
        /// </summary>
        [DataMember(Name = "@type", Order = 1)]
        [JsonProperty(Required = Required.DisallowNull)]
        public string Type => "Gebouw";

        /// <summary>
        /// De identificator van het gebouw.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouwIdentificator Identificator { get; set; }

        /// <summary>
        /// the building geometry (a simple polygon with Lambert-72 coordinates)
        /// </summary>
        [DataMember(Name = "GebouwPolygoon", Order = 3, EmitDefaultValue = true)]
        public BuildingPolygon BuildingPolygon { get; set; }

        /// <summary>
        /// De fase in het leven van een gebouw.
        /// </summary>
        [DataMember(Name = "GebouwStatus", Order = 4)]
        [JsonProperty(Required = Required.DisallowNull)]
        public GebouwStatus Status { get; set; }

        /// <summary>
        /// De aan het gebouw gekoppelde gebouweenheden.
        /// </summary>
        [DataMember(Name = "Gebouweenheden", Order = 5)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<GebouwDetailGebouweenheid> BuildingUnits { get; set; }

        /// <summary>
        /// De aan het gebouw gekoppelde percelen.
        /// </summary>
        [DataMember(Name = "Percelen", Order = 6)]
        [JsonProperty(Required = Required.DisallowNull)]
        public List<GebouwDetailPerceel> Parcels { get; set; }

        public BuildingOsloResponse(
            int persistentLocalId,
            string naamruimte,
            string contextUrlDetail,
            DateTimeOffset version,
            BuildingPolygon buildingPolygon,
            GebouwStatus status,
            List<GebouwDetailGebouweenheid> buildingUnits,
            List<GebouwDetailPerceel> parcels)
        {
            Context = contextUrlDetail;
            Identificator = new GebouwIdentificator(naamruimte, persistentLocalId.ToString(), version);
            BuildingPolygon = buildingPolygon;
            Status = status;
            BuildingUnits = buildingUnits;
            Parcels = parcels;
        }
    }

    public class BuildingOsloResponseExamples : IExamplesProvider<BuildingOsloResponse>
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingOsloResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public BuildingOsloResponse GetExamples()
        {
            var buildingPolygon = new BuildingPolygon(
                new GmlJsonPolygon("<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>101673.0 193520.0 101673.0 193585.0 101732.0 193585.0 101673.0 193585.0 101673.0 193520.0</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>"),
                GeometrieMethode.IngemetenGRB);

            return new BuildingOsloResponse(
                6,
                _responseOptions.GebouwNaamruimte,
                _responseOptions.ContextUrlDetail,
                DateTimeOffset.Now.ToExampleOffset(),
                buildingPolygon,
                GebouwStatus.Gerealiseerd,
                new List<GebouwDetailGebouweenheid>
                {
                    new GebouwDetailGebouweenheid("1", string.Format(_responseOptions.GebouweenheidDetailUrl,"1")),
                    new GebouwDetailGebouweenheid("2", string.Format(_responseOptions.GebouweenheidDetailUrl,"2"))
                },
                new List<GebouwDetailPerceel>
                {
                    new GebouwDetailPerceel("11001B0008-00G002",string.Format(_responseOptions.PerceelUrl,"11001B0008-00G002")),
                    new GebouwDetailPerceel("11001B0008-00G003", string.Format(_responseOptions.PerceelUrl,"11001B0008-00G003"))
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
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext)
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
                ProblemInstanceUri = _problemDetailsHelper.GetInstanceUri(_httpContextAccessor.HttpContext)
            };
    }
}
