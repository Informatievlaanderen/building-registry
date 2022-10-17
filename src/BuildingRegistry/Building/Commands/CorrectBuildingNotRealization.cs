namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class CorrectBuildingNotRealization : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("7f52a1ad-2f76-419e-b2af-c84a41b9c447");

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

        public Provenance Provenance { get; }

        public CorrectBuildingNotRealization(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CorrectBuildingNotRealization-{ToString()}");

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
