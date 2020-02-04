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
    using Microsoft.Extensions.Options;

    [DataContract(Name = "GebouweenheidDetail", Namespace = "")]
    public class BuildingUnitResponse
    {
        /// <summary>
        /// De identificator van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 1)]
        public GebouweenheidIdentificator Identificator { get; set; }

        /// <summary>
        /// The building unit geometry (a point with Lambert-72 coordinates)
        /// </summary>
        [DataMember(Name = "GeometriePunt", Order = 2)]
        public Point Geometry { get; set; }

        /// <summary>
        /// De gebruikte methode om de positie te bepalen.
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode", Order = 3)]
        public PositieGeometrieMethode GeometryMethod { get; set; }

        /// <summary>
        /// De fase in het leven van een gebouweenheid.
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
        /// De aan de gebouweenheid gelinkte adressen.
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
                DateTimeOffset.Now,
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
                new GebouweenheidDetailGebouw("1", "http://baseuri/api/gebouw/1"),
                new List<GebouweenheidDetailAdres>
                {
                    new GebouweenheidDetailAdres("1", "http://baseuri/api/adres/1"),
                    new GebouweenheidDetailAdres("7", "http://baseuri/api/adres/7"),
                    new GebouweenheidDetailAdres("10", "http://baseuri/api/adres/10")
                }
            );
    }

    public class BuildingUnitNotFoundResponseExamples : IExamplesProvider<ProblemDetails>
    {
        public ProblemDetails GetExamples()
            => new ProblemDetails
            {
                HttpStatus = StatusCodes.Status404NotFound,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Onbestaande gebouweenheid.",
                ProblemInstanceUri = new DefaultHttpContext().GetProblemInstanceUri()
            };
    }

    public class BuildingUnitGoneResponseExamples : IExamplesProvider<ProblemDetails>
    {
        public ProblemDetails GetExamples()
            => new ProblemDetails
            {
                HttpStatus = StatusCodes.Status410Gone,
                Title = ProblemDetails.DefaultTitle,
                Detail = "Gebouweenheid werd verwijderd.",
                ProblemInstanceUri = new DefaultHttpContext().GetProblemInstanceUri()
            };
    }
}
