namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using MediatR;
    using Newtonsoft.Json;
    using Responses;

    public class PlaceBuildingUnderConstructionRequest : PlaceBuildingUnderConstructionBackOfficeRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public PlaceBuildingUnderConstruction ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            return new PlaceBuildingUnderConstruction(
                buildingPersistentLocalId,
                provenance);
        }
    }
}
