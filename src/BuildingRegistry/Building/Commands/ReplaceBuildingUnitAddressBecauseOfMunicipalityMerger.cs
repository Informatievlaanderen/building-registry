namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("87cc2af0-05ba-4f56-9cef-5554dc4ad352");

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }
        public BuildingUnitPersistentLocalId BuildingUnitPersistentLocalId { get; }
        public AddressPersistentLocalId NewAddressPersistentLocalId { get; }
        public AddressPersistentLocalId PreviousAddressPersistentLocalId { get; }
        public Provenance Provenance { get; }

        public ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId newAddressPersistentLocalId,
            AddressPersistentLocalId previousAddressPersistentLocalId,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            PreviousAddressPersistentLocalId = previousAddressPersistentLocalId;
            NewAddressPersistentLocalId = newAddressPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ReplaceBuildingUnitAddressBecauseOfMunicipalityMerger-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;
            yield return BuildingUnitPersistentLocalId;
            yield return NewAddressPersistentLocalId;
            yield return PreviousAddressPersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
