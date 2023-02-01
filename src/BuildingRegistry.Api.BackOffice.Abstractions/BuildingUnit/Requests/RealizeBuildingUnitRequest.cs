namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Newtonsoft.Json;

    public sealed class RealizeBuildingUnitRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public RealizeBuildingUnit ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            Provenance provenance)
        {
            return new RealizeBuildingUnit(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                provenance);
        }
    }
}
