namespace BuildingRegistry.Api.Legacy.Building.Responses
{
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Infrastructure.Options;
    using Microsoft.AspNetCore.Http;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;

    [DataContract(Name = "GebouwDetail", Namespace = "")]
    public class BuildingResponse
    {
        /// <summary>
        /// De identificator van het gebouw.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        public Identificator Identificator { get; set; }

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
            int osloId,
            string naamruimte,
            DateTimeOffset version,
            Polygon geometry,
            GeometrieMethode geometryMethod,
            GebouwStatus status,
            List<GebouwDetailGebouweenheid> buildingUnits,
            List<GebouwDetailPerceel> parcels)
        {
            Identificator = new Identificator(naamruimte, osloId.ToString(), version);
            Polygon = geometry;
            GeometryMethod = geometryMethod;
            Status = status;
            BuildingUnits = buildingUnits;
            Parcels = parcels;
        }
    }

    public class BuildingResponseExamples : IExamplesProvider
    {
        private readonly ResponseOptions _options;

        public BuildingResponseExamples(ResponseOptions options) => _options = options;

        public object GetExamples()
            => new BuildingResponse(
                6,
                _options.GebouwNaamruimte,
                DateTimeOffset.Now,
                new Polygon
                {
                    JsonPolygon = new GeoJSONPolygon
                    {
                        Coordinates = new double[][][] { new double[][] { new double[] { 101673.0, 193520.0 }, new double[] { 101673.0, 193585.0 }, new double[] { 101732.0, 193585.0 }, new double[] { 101673.0, 193585.0 }, new double[] { 101673.0, 193520.0 } } },
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
