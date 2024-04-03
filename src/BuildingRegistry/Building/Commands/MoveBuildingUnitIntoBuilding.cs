namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class MoveBuildingUnitIntoBuilding : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("a0c0892b-8448-4b56-b233-16eae06995d6");
        
        public BuildingPersistentLocalId SourceBuildingPersistentLocalId { get; }
        public BuildingPersistentLocalId DestinationBuildingPersistentLocalId { get; }
        public BuildingUnitPersistentLocalId BuildingUnitPersistentLocalId { get; }
        public Provenance Provenance { get; }

        public MoveBuildingUnitIntoBuilding(
            BuildingPersistentLocalId sourceBuildingPersistentLocalId,
            BuildingPersistentLocalId destinationBuildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            Provenance provenance)
        {
            SourceBuildingPersistentLocalId = sourceBuildingPersistentLocalId;
            DestinationBuildingPersistentLocalId = destinationBuildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"MoveBuildingUnitIntoBuilding-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return SourceBuildingPersistentLocalId;
            yield return DestinationBuildingPersistentLocalId;
            yield return BuildingUnitPersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
