namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using MediatR;
    using Newtonsoft.Json;
    
    public class CorrectBuildingUnitRealizationRequest : BackOfficeCorrectBuildingUnitRealizationRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public CorrectBuildingUnitRealization ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            Provenance provenance)
        {
            return new CorrectBuildingUnitRealization(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                provenance);
        }
    }
}
