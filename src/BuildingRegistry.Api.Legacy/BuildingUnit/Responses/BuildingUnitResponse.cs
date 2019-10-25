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
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.BasicApiProblem;
    using Microsoft.Extensions.Options;

    [DataContract(Name = "GebouweenheidDetail", Namespace = "")]
    public class BuildingUnitResponse
    {
        /// <summary>
        /// De identificator van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        public Identificator Identificator { get; set; }

        /// <summary>
        /// The building unit geometry (a point with Lambert-72 coordinates)
        /// </summary>
        [DataMember(Name = "GeometriePunt", Order = 2)]
        public Point Point { get; set; }

        /// <summary>
        /// the method used to provide the position
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode", Order = 3)]
        public PositieGeometrieMethode GeometryMethod { get; set; }

        /// <summary>
        /// the current phase in the lifecycle of the building unit
        /// </summary>
        [DataMember(Name = "GebouweenheidStatus", Order = 4)]
        public GebouweenheidStatus Status { get; set; }

        /// <summary>
        /// the function of the building unit in reality (as observed on site)
        /// </summary>
        [DataMember(Name = "Functie", Order = 5)]
        public GebouweenheidFunctie? Function { get; set; }

        /// <summary>
        /// building wherein the building unit resides
        /// </summary>
        [DataMember(Name = "Gebouw", Order = 6)]
        public GebouweenheidDetailGebouw Building { get; set; }

        /// <summary>
        /// a collection of addresses that are coupled to the building unit
        /// </summary>
        [DataMember(Name = "Adressen", Order = 7)]
        public List<GebouweenheidDetailAdres> Addresses { get; set; }

        public BuildingUnitResponse(
            int persistentLocalId,
            string naamruimte,
            DateTimeOffset version,
            Point geometry,
            PositieGeometrieMethode geometryMethod,
            GebouweenheidStatus status,
            GebouweenheidFunctie? functie,
            GebouweenheidDetailGebouw building,
            List<GebouweenheidDetailAdres> addresses)
        {
            Identificator = new Identificator(naamruimte, persistentLocalId.ToString(), version);
            Point = geometry;
            GeometryMethod = geometryMethod;
            Status = status;
            Function = functie;
            Building = building;
            Addresses = addresses;
        }
    }

    public class BuildingUnitResponseExamples : IExamplesProvider
    {
        private readonly ResponseOptions _responseOptions;

        public BuildingUnitResponseExamples(IOptions<ResponseOptions> responseOptionsProvider) => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples()
            => new BuildingUnitResponse
            (
                6,
                _responseOptions.GebouweenheidNaamruimte,
                DateTimeOffset.Now,
                new Point
                {
                    JsonPoint = new GeoJSONPoint
                    {
                        Type = "point",
                        Coordinates = new double[] { 140252.76, 198794.27 }
                    },
                    XmlPoint = new GmlPoint
                    {
                        Pos = "140252.76 198794.27"
                    }
                },
                PositieGeometrieMethode.AangeduidDoorBeheerder,
                GebouweenheidStatus.Gerealiseerd,
                GebouweenheidFunctie.GemeenschappelijkDeel,
                new GebouweenheidDetailGebouw("1", "http://baseuri/api/gebouw/1"),
                new List<GebouweenheidDetailAdres>
                {
                    new GebouweenheidDetailAdres("1", "http://baseuri/api/adres/1"),
                    new GebouweenheidDetailAdres("7", "http://baseuri/api/adres/7"),
                    new GebouweenheidDetailAdres("10", "http://baseuri/api/adres/10")
                }
            );
    }

    public class BuildingUnitNotFoundResponseExamples : IExamplesProvider
    {
        public object GetExamples()
            => new ProblemDetails
            {
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Onbestaande gebouweenheid.",
                ProblemInstanceUri = ProblemDetails.GetProblemNumber()
            };
    }

    public class BuildingUnitGoneResponseExamples : IExamplesProvider
    {
        public object GetExamples()
            => new ProblemDetails
            {
                HttpStatus = StatusCodes.Status410Gone,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Gebouweenheid werd verwijderd.",
                ProblemInstanceUri = ProblemDetails.GetProblemNumber()
            };
    }
}
