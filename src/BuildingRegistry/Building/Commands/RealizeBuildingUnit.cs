namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class RealizeBuildingUnit : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("7ce3ab7a-99f7-4658-9f54-169c8de7327b");
        
        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }
        public BuildingUnitPersistentLocalId BuildingUnitPersistentLocalId { get; }
        public Provenance Provenance { get; }

        public RealizeBuildingUnit(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RealizeBuildingUnit-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;
            yield return BuildingUnitPersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
