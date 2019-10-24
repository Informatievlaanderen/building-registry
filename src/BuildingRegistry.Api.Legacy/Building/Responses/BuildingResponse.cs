namespace BuildingRegistry.Api.Legacy.Building.Responses
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "GebouwDetail", Namespace = "")]
    public class BuildingResponse
    {
        /// <summary>
        /// De identificator van het gebouw.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        public GebouwIdentificator Identificator { get; set; }

        /// <summary>
        /// the building geometry (a simple polygon with Lambert-72 coordinates)
        /// </summary>
        [DataMember(Name = "GeometriePolygoon", Order = 2)]
        public Polygon Polygon { get; set; }

        /// <summary>
        /// the method used to provide the geometry
        /// </summary>
        [DataMember(Name = "GeometrieMethode", Order = 3)]
        public GeometrieMethode GeometryMethod { get; set; }

        /// <summary>
        /// the current phase in the lifecycle of the building
        /// </summary>
        [DataMember(Name = "GebouwStatus", Order = 4)]
        public GebouwStatus Status { get; set; }

        /// <summary>
        /// a collection of building units that reside within the building
        /// </summary>
        [DataMember(Name = "Gebouweenheden", Order = 5)]
        public List<GebouwDetailGebouweenheid> BuildingUnits { get; set; }

        /// <summary>
        /// a collection of parcels that lie underneath the building
        /// </summary>
        [DataMember(Name = "Percelen", Order = 6)]
        public List<GebouwDetailPerceel> Parcels { get; set; }

        public BuildingResponse(
            int persistentLocalId,
            string naamruimte,
            DateTimeOffset version,
            Polygon geometry,
            GeometrieMethode geometryMethod,
            GebouwStatus status,
            List<GebouwDetailGebouweenheid> buildingUnits,
            List<GebouwDetailPerceel> parcels)
        {
            Identificator = new GebouwIdentificator(naamruimte, persistentLocalId.ToString(), version);
            Polygon = geometry;
            GeometryMethod = geometryMethod;
            Status = status;
            BuildingUnits = buildingUnits;
            Parcels = parcels;
        }
    }

    public class BuildingResponseExamples : IExamplesProvider
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples()
            => new BuildingResponse(
                6,
                _responseOptions.GebouwNaamruimte,
                DateTimeOffset.Now,
                new Polygon
                {
                    JsonPolygon = new GeoJSONPolygon
                    {
                        Coordinates = new[] { new[] { new[] { 101673.0, 193520.0 }, new[] { 101673.0, 193585.0 }, new[] { 101732.0, 193585.0 }, new[] { 101673.0, 193585.0 }, new[] { 101673.0, 193520.0 } } },
                        Type = "Polygon"
                    },
                    XmlPolygon = new GmlPolygon
                    {
                        Exterior = new RingProperty
                        {
                            LinearRing = new LinearRing
                            {
                                PosList = "101673.0 193520.0 101673.0 193585.0 101732.0 193585.0 101673.0 193585.0 101673.0 193520.0"
                            }
                        }
                    }
                },
                GeometrieMethode.IngemetenGRB,
                GebouwStatus.Gerealiseerd,
                new List<GebouwDetailGebouweenheid>
                {
                    new GebouwDetailGebouweenheid("1", "http://baseuri/api/gebouweenheid/1"),
                    new GebouwDetailGebouweenheid("2", "http://baseuri/api/gebouweenheid/2")
                },
                new List<GebouwDetailPerceel>
                {
                    new GebouwDetailPerceel("11001B0008-00G002", "http://baseuri/api/perceel/1/11001B0008-00G002"),
                    new GebouwDetailPerceel("11001B0008-00G003", "http://baseuri/api/perceel/1/11001B0008-00G003")
                });
    }

    public class BuildingNotFoundResponseExamples : IExamplesProvider
    {
        public object GetExamples()
            => new ProblemDetails
            {
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Onbestaand gebouw.",
                ProblemInstanceUri = ProblemDetails.GetProblemNumber()
            };
    }

    public class BuildingGoneResponseExamples : IExamplesProvider
    {
        public object GetExamples()
            => new ProblemDetails
            {
                HttpStatus = StatusCodes.Status410Gone,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Gebouw werd verwijderd.",
                ProblemInstanceUri = ProblemDetails.GetProblemNumber()
            };
    }
}
