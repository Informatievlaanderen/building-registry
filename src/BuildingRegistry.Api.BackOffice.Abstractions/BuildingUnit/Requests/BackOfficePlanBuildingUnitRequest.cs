namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Newtonsoft.Json;
    using System.Runtime.Serialization;

    [DataContract(Name = "PlanGebouweenheid", Namespace = "")]
    public class BackOfficePlanBuildingUnitRequest
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
        [DataMember(Name = "PositieGeometriemethode", Order = 1)]
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
}
