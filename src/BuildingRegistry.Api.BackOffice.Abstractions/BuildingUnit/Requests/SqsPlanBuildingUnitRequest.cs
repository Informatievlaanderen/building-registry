namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using MediatR;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    public class SqsPlanBuildingUnitRequest : IRequest<Unit>
    {
        /// <summary>
        /// Identificator van het gebouw
        /// </summary>
        [DataMember(Name = "GebouwId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string GebouwId { get; set; }

        /// <summary>
        /// De geometriemethode van de gebouweenheidpositie. 
        /// </summary>
        [DataMember(Name = "PositieGeometriemethode", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public PositieGeometrieMethode PositieGeometrieMethode { get; set; }

        /// <summary>
        /// Puntgeometrie van de gebouweenheid binnen het gebouw.
        /// </summary>
        [DataMember(Name = "Positie", Order = 2)]
        [JsonProperty(Required = Required.Default)]
        public string? Positie { get; set; }

        /// <summary>
        /// De functie van de gebouweenheid.
        /// </summary>
        [DataMember(Name = "Functie", Order = 3)]
        [JsonProperty(Required = Required.Always)]
        public GebouweenheidFunctie Functie { get; set; }

        /// <summary>
        /// Wanneer de definitie van een gebouweenheid niet werd gevolgd en dus ‘afwijkend’ is.
        /// </summary>
        [DataMember(Name = "AfwijkingVastgesteld", Order = 4)]
        [JsonProperty(Required = Required.Always)]
        public bool AfwijkingVastgesteld { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        [JsonIgnore]
        public string? MessageGroupId { get; set; }
    }

    public class SqsPlanBuildingUnitRequestExamples : IExamplesProvider<SqsPlanBuildingUnitRequest>
    {
        public SqsPlanBuildingUnitRequest GetExamples()
        {
            return new SqsPlanBuildingUnitRequest
            {
                GebouwId = "https://data.vlaanderen.be/id/gebouw/6447380",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3137\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
                Functie = GebouweenheidFunctie.NietGekend,
                AfwijkingVastgesteld = false
            };
        }
    }

}
