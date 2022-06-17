namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class PlaceBuildingUnderConstruction : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("267cbf1a-5c95-4e84-829b-d5ea6a0fadc8");
        
        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

        public Provenance Provenance { get; }

        public PlaceBuildingUnderConstruction(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"PlaceBuildingUnderConstruction-{ToString()}");

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
