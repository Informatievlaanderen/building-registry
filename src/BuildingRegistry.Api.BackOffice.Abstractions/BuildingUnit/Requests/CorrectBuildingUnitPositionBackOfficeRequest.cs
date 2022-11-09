using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    [DataContract(Name = "CorrigeerGebouweenheidPositie", Namespace = "")]
    public class CorrectBuildingUnitPositionBackOfficeRequest
    {
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
    }
}
