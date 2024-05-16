namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ReaddressAddresses : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("646d3ef7-6cbc-4b33-b75f-e5d72e48c356");
        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }
        public Dictionary<BuildingUnitPersistentLocalId, IEnumerable<ReaddressData>> Readdresses { get; }
        public Provenance Provenance { get; }

        public ReaddressAddresses(
            BuildingPersistentLocalId buildingPersistentLocalId,
            Dictionary<BuildingUnitPersistentLocalId, IEnumerable<ReaddressData>> readdresses,
            Provenance provenance)
        {

            BuildingPersistentLocalId = buildingPersistentLocalId;
            Readdresses = readdresses;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ReaddressAddresses-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return BuildingPersistentLocalId;

            foreach (var address in Readdresses)
            {
                yield return address.Key;
                foreach (var readdressData in address.Value)
                {
                    yield return readdressData.SourceAddressPersistentLocalId;
                    yield return readdressData.DestinationAddressPersistentLocalId;
                }
            }

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }

    public class ReaddressData
    {
        public AddressPersistentLocalId SourceAddressPersistentLocalId { get; }
        public AddressPersistentLocalId DestinationAddressPersistentLocalId { get; }

        public ReaddressData(
            AddressPersistentLocalId sourceAddressPersistentLocalId,
            AddressPersistentLocalId destinationAddressPersistentLocalId)
        {
            SourceAddressPersistentLocalId = sourceAddressPersistentLocalId;
            DestinationAddressPersistentLocalId = destinationAddressPersistentLocalId;
        }
    }
}
