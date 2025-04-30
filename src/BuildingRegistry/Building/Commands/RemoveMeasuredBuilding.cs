namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class RemoveMeasuredBuilding : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("4d0921a7-5f3a-41e3-82b6-3943c6f14157");

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

        public Provenance Provenance { get; }

        public RemoveMeasuredBuilding(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RemoveMeasuredBuilding-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
