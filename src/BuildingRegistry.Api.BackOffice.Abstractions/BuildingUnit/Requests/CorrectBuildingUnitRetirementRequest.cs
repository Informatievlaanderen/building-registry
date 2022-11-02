namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using MediatR;
    using Newtonsoft.Json;

    public class CorrectBuildingUnitRetirementRequest : CorrectBuildingUnitRetirementBackOfficeRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public CorrectBuildingUnitRetirement ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            Provenance provenance)
        {
            return new CorrectBuildingUnitRetirement(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                provenance);
        }
    }
}
