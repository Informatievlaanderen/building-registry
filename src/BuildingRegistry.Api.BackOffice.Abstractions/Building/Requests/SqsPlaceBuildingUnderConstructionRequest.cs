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

    [DataContract(Name = "GebouwInAanbouw", Namespace = "")]
    public class SqsPlaceBuildingUnderConstructionRequest : IRequest<Unit>
    {
        public int PersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        [JsonIgnore]
        public Guid TicketId { get; set; }

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
