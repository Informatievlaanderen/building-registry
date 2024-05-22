namespace BuildingRegistry.Building.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ReaddressAddresses : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("444c245f-b4ab-463d-bbe2-c88769953481");
        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }
        public IReadOnlyDictionary<BuildingUnitPersistentLocalId, IReadOnlyList<ReaddressData>> Readdresses { get; }
        public Provenance Provenance { get; }

        public ReaddressAddresses(
            BuildingPersistentLocalId buildingPersistentLocalId,
            IReadOnlyDictionary<BuildingUnitPersistentLocalId, IReadOnlyList<ReaddressData>> readdresses,
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

            foreach (var buildingUnitReaddress in Readdresses)
            {
                yield return buildingUnitReaddress.Key;
                foreach (var readdressData in buildingUnitReaddress.Value)
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
