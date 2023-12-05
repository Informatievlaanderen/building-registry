namespace BuildingRegistry.Legacy.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class MarkBuildingAsMigrated : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("90d3c976-3fa8-4f93-8c73-fef103706e05");

        public BuildingId BuildingId { get; }
        public PersistentLocalId PersistentLocalId { get; set; }
        public Provenance Provenance { get; }

        public MarkBuildingAsMigrated(
            BuildingId buildingId,
            PersistentLocalId persistentLocalId,
            Provenance provenance)
        {
            BuildingId = buildingId;
            PersistentLocalId = persistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"MarkBuildingAsMigrated-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingId;
        }
    }
}
