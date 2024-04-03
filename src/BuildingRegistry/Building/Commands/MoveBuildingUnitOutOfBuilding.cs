namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class MoveBuildingUnitOutOfBuilding : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("0feb6f31-b0b1-4722-861b-d14ce42e160a");
        
        public BuildingPersistentLocalId SourceBuildingPersistentLocalId { get; }
        public BuildingPersistentLocalId DestinationBuildingPersistentLocalId { get; }
        public BuildingUnitPersistentLocalId BuildingUnitPersistentLocalId { get; }
        public Provenance Provenance { get; }

        public MoveBuildingUnitOutOfBuilding(
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
            => Deterministic.Create(Namespace, $"MoveBuildingUnitOutOfBuilding-{ToString()}");

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
