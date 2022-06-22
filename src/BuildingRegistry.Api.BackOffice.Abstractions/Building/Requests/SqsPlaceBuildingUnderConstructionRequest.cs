namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using MediatR;

    public class SqsPlaceBuildingUnderConstructionRequest : IRequest<Unit>
    {
        public int PersistentLocalId { get; set; }

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
