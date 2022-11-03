namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class CorrectBuildingPlaceUnderConstruction: IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("09b7da93-fede-4908-a8ee-c270ca859e79");
        
        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

        public Provenance Provenance { get; }

        public CorrectBuildingPlaceUnderConstruction(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CorrectBuildingPlaceUnderConstruction-{ToString()}");

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
