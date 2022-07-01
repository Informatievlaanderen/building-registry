namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using MediatR;
    using Newtonsoft.Json;

    [DataContract(Name = "RealiseerGebouw", Namespace = "")]
    public class SqsRealizeBuildingRequest : IRequest<Unit>
    {
        public int PersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        [JsonIgnore]
        public Guid TicketId { get; set; }

        public RealizeBuilding ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            return new RealizeBuilding(
                buildingPersistentLocalId,
                provenance);
        }
    }
}
