namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Newtonsoft.Json;

    public sealed class CorrectBuildingRealizationRequest
    {
        public int PersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public CorrectBuildingRealization ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            return new CorrectBuildingRealization(
                buildingPersistentLocalId,
                provenance);
        }
    }
}
