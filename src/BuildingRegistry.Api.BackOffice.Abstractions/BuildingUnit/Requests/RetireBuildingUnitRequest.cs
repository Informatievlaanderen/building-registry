namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Newtonsoft.Json;

    public sealed class RetireBuildingUnitRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public RetireBuildingUnit ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            return new RetireBuildingUnit(
                buildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                provenance);
        }
    }
}
