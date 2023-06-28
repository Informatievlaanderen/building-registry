namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Newtonsoft.Json;

    public sealed class CorrectBuildingNotRealizationRequest
    {
        public int PersistentLocalId { get; set; }

        public CorrectBuildingNotRealization ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            return new CorrectBuildingNotRealization(
                buildingPersistentLocalId,
                provenance);
        }
    }
}
