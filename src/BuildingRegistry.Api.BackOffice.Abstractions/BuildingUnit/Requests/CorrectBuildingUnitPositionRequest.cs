namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "CorrigeerGebouweenheidPositie", Namespace = "")]
    public sealed class CorrectBuildingUnitPositionRequest
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

        [JsonIgnore]
        public int BuildingUnitPersistentLocalId { get; set; }
    }

    public class CorrectBuildingUnitPositionRequestExamples : IExamplesProvider<CorrectBuildingUnitPositionRequest>
    {
        public CorrectBuildingUnitPositionRequest GetExamples()
        {
            return new CorrectBuildingUnitPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = " < gml:Point srsName =\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>"
            };
        }
    }
}
