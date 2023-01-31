namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class ChangeBuildingUnitFunction : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("0b98bd67-ac41-49cd-81c8-5ab3d8e432b0");

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }
        public BuildingUnitPersistentLocalId BuildingUnitPersistentLocalId { get; }
        public BuildingUnitFunction Function { get; }

        public Provenance Provenance { get; }

        public ChangeBuildingUnitFunction(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            BuildingUnitFunction function,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            Function = function;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ChangeBuildingUnitFunction-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;
            yield return BuildingUnitPersistentLocalId;
            yield return Function.ToString() ?? string.Empty;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
