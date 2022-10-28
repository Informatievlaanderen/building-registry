namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using MediatR;
    using Newtonsoft.Json;

    public class RetireBuildingUnitRequest : RetireBuildingUnitBackOfficeRequest, IRequest<ETagResponse>
    {
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
