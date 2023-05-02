namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Newtonsoft.Json;

    public sealed class DemolishBuildingRequest
    {
        public GrbData GrbData { get; set; }

        public DemolishBuilding ToCommand(int PersistentLocalId, Provenance provenance)
            => new DemolishBuilding(new BuildingPersistentLocalId(PersistentLocalId),
                GrbData.ToBuildingGrbData(),
                provenance);
    }
}
