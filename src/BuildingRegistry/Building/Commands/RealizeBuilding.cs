namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class RealizeBuilding : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("8b055fde-6e15-4962-bed2-2ca61359ee87");
        
        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

        public Provenance Provenance { get; }

        public RealizeBuilding(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RealizeBuilding-{ToString()}");

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
