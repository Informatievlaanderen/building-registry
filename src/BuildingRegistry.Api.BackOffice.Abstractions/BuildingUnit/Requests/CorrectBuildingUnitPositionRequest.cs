namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Converters;
    using Newtonsoft.Json;
    using Swashbuckle.AspNetCore.Filters;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

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

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public CorrectBuildingUnitPosition ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            Provenance provenance)
        {
            return new CorrectBuildingUnitPosition(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                PositieGeometrieMethode.Map(),
                string.IsNullOrWhiteSpace(Positie) ? null : Positie.ToExtendedWkbGeometry(),
                provenance);
        }
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
