namespace BuildingRegistry.Producer.Extensions
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.BuildingRegistry;
    using Legacy = Legacy.Events;
    using BuildingAggregate = Building.Events;

    public static class MessageExtensions
    {
        #region Legacy

        public static Contracts.BuildingAddressWasAttached ToContract(this Legacy.BuildingAddressWasAttached message) =>
            new Contracts.BuildingAddressWasAttached(message.BuildingId.ToString("D"), message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingAddressWasDetached ToContract(this Legacy.BuildingAddressWasDetached message) =>
            new Contracts.BuildingAddressWasDetached(message.BuildingId.ToString("D"), message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingWasCorrectedToRealized ToContract(this Legacy.BuildingWasCorrectedToRealized message) =>
            new Contracts.BuildingWasCorrectedToRealized(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingWasCorrectedToRetired ToContract(this Legacy.BuildingWasCorrectedToRetired message) =>
            new Contracts.BuildingWasCorrectedToRetired(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingWasMarkedAsMigrated ToContract(this Legacy.BuildingWasMarkedAsMigrated message) =>
            new Contracts.BuildingWasMarkedAsMigrated(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingWasRecovered ToContract(this Legacy.BuildingWasRecovered message) =>
            new Contracts.BuildingWasRecovered(message.BuildingId.ToString("D"), message.Provenance.ToContract());

         public static Contracts.BuildingWasRemoved ToContract(this Legacy.BuildingWasRemoved message) =>
            new Contracts.BuildingWasRemoved(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingWasRetired ToContract(this Legacy.BuildingWasRetired message) =>
            new Contracts.BuildingWasRetired(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingWasRealized ToContract(this Legacy.BuildingWasRealized message) =>
            new Contracts.BuildingWasRealized(message.BuildingId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.BuildingWasRegistered ToContract(this Legacy.BuildingWasRegistered message) =>
            new Contracts.BuildingWasRegistered(message.BuildingId.ToString("D"), message.VbrCaPaKey, message.Provenance.ToContract());

        #endregion

        public static Contracts.BuildingAddressWasAttachedV2 ToContract(this BuildingAggregate.BuildingAddressWasAttachedV2 message) =>
            new Contracts.BuildingAddressWasAttachedV2(message.BuildingId.ToString("D"), message.CaPaKey, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.BuildingAddressWasDetachedV2 ToContract(this BuildingAggregate.BuildingAddressWasDetachedV2 message) =>
            new Contracts.BuildingAddressWasDetachedV2(message.BuildingId.ToString("D"), message.CaPaKey, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.BuildingAddressWasDetachedBecauseAddressWasRejected ToContract(this BuildingAggregate.BuildingAddressWasDetachedBecauseAddressWasRejected message) =>
            new Contracts.BuildingAddressWasDetachedBecauseAddressWasRejected(message.BuildingId.ToString("D"), message.CaPaKey, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.BuildingAddressWasDetachedBecauseAddressWasRetired ToContract(this BuildingAggregate.BuildingAddressWasDetachedBecauseAddressWasRetired message) =>
            new Contracts.BuildingAddressWasDetachedBecauseAddressWasRetired(message.BuildingId.ToString("D"), message.CaPaKey, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.BuildingAddressWasDetachedBecauseAddressWasRemoved ToContract(this BuildingAggregate.BuildingAddressWasDetachedBecauseAddressWasRemoved message) =>
            new Contracts.BuildingAddressWasDetachedBecauseAddressWasRemoved(message.BuildingId.ToString("D"), message.CaPaKey, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.BuildingWasMigrated ToContract(this BuildingAggregate.BuildingWasMigrated message) =>
            new Contracts.BuildingWasMigrated(message.OldBuildingId.ToString("D"), message.BuildingId.ToString("D"), message.CaPaKey, message.BuildingStatus, message.IsRemoved, message.AddressPersistentLocalIds, message.XCoordinate, message.YCoordinate, message.Provenance.ToContract());

        private static Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance ToContract(this ProvenanceData provenance)
        => new Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance(
            provenance.Timestamp.ToString(),
            provenance.Application.ToString(),
            provenance.Modification.ToString(),
            provenance.Organisation.ToString(),
            provenance.Reason);
    }
}
