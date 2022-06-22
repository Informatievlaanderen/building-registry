namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using MediatR;

    public class SqsRealizeBuildingRequest : IRequest<Unit>
    {
        public int PersistentLocalId { get; set; }

        public IDictionary<string, object> Metadata { get; set; }

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
