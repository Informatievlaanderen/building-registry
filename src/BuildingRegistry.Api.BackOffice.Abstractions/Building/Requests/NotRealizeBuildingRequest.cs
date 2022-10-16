namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using MediatR;
    using Newtonsoft.Json;

    public class NotRealizeBuildingRequest : BackOfficeNotRealizeBuildingRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        public NotRealizeBuilding ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            return new NotRealizeBuilding(
                buildingPersistentLocalId,
                provenance);
        }
    }
}
