namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "PlanGebouweenheid", Namespace = "")]
    public sealed class PlanBuildingUnitRequest
    {
        /// <summary>
        /// Identificator van het gebouw.
        /// </summary>
        [DataMember(Name = "GebouwId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string GebouwId { get; set; }

        /// <summary>
        /// De geometriemethode van de gebouweenheidpositie.
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public PositieGeometrieMethode PositieGeometrieMethode { get; set; }

        /// <summary>
        /// Puntgeometrie van de gebouweenheid binnen het gebouw in GML-3 formaat met Lambert 72 referentie systeem.
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
    }

    public class PlanBuildingUnitRequestExamples : IExamplesProvider<PlanBuildingUnitRequest>
    {
        public PlanBuildingUnitRequest GetExamples()
        {
            return new PlanBuildingUnitRequest
            {
                GebouwId = "https://data.vlaanderen.be/id/gebouw/6447380",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
                Functie = GebouweenheidFunctie.NietGekend,
                AfwijkingVastgesteld = false
            };
        }
    }
}
