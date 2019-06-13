namespace BuildingRegistry.ValueObjects
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;

    public class AddressId : GuidValueObject<AddressId>
    {
        public static AddressId CreateFor(CrabHouseNumberId crabHouseNumberId)
            => new AddressId(crabHouseNumberId.CreateDeterministicId());

        public static AddressId CreateFor(CrabSubaddressId crabSubaddressId)
            => new AddressId(crabSubaddressId.CreateDeterministicId());

        public AddressId(Guid addressId) : base(addressId) { }
    }
}
