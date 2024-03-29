namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Datastructures;

    public sealed class DemolishBuilding : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("3b26dc1a-661a-4ee4-acba-77ff083a878a");

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }
        public BuildingGrbData BuildingGrbData { get; }

        public Provenance Provenance { get; }

        public DemolishBuilding(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingGrbData buildingGrbData,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingGrbData = buildingGrbData;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"DemolishBuilding-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;

            foreach (var field in BuildingGrbData.IdentityFields())
            {
                yield return field;
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
