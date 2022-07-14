namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    [DataContract(Name = "RealiseerGebouweenheid", Namespace = "")]
    public class SqsRealizeBuildingUnitRequest : IRequest<IResult>
    {
        public int PersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        [JsonIgnore]
        public string? MessageGroupId { get; set; }

        [JsonIgnore]
        public Guid TicketId { get; set; }

        public RealizeBuildingUnit ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            Provenance provenance)
        {
            return new RealizeBuildingUnit(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                provenance);
        }
    }
}
