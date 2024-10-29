namespace BuildingRegistry.AllStream.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using Building;

    public sealed class CreateOsloSnapshots : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("2626bb06-4602-4864-a7c5-71f3a884af69");

        public IReadOnlyList<BuildingPersistentLocalId> BuildingPersistentLocalIds { get; }
        public IReadOnlyList<BuildingUnitPersistentLocalId> BuildingUnitPersistentLocalIds { get; }

        public Provenance Provenance { get; }

        public CreateOsloSnapshots(
            IEnumerable<BuildingPersistentLocalId> buildingPersistentLocalIds,
            IEnumerable<BuildingUnitPersistentLocalId> buildingUnitPersistentLocalIds,
            Provenance provenance)
        {
            BuildingPersistentLocalIds = buildingPersistentLocalIds.ToList();
            BuildingUnitPersistentLocalIds = buildingUnitPersistentLocalIds.ToList();
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CreateOsloSnapshots-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalIds;
            yield return BuildingUnitPersistentLocalIds;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
