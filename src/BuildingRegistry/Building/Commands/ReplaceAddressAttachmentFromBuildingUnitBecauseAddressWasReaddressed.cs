namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class ReplaceAddressAttachmentFromBuildingUnitBecauseAddressWasReaddressed : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("d28b22de-e729-48da-8ee7-ac20fff8f7b9");

        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }
        public BuildingUnitPersistentLocalId BuildingUnitPersistentLocalId { get; }
        public AddressPersistentLocalId PreviousAddressPersistentLocalId { get; }
        public AddressPersistentLocalId NewAddressPersistentLocalId { get; }
        public Provenance Provenance { get; }

        public ReplaceAddressAttachmentFromBuildingUnitBecauseAddressWasReaddressed(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId,
            AddressPersistentLocalId previousAddressPersistentLocalId,
            AddressPersistentLocalId newAddressPersistentLocalId,
            Provenance provenance)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            PreviousAddressPersistentLocalId = previousAddressPersistentLocalId;
            NewAddressPersistentLocalId = newAddressPersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ReplaceAddressAttachmentFromBuildingUnitBecauseAddressWasReaddressed-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;
            yield return BuildingUnitPersistentLocalId;
            yield return PreviousAddressPersistentLocalId;
            yield return NewAddressPersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
