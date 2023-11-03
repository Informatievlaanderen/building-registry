namespace BuildingRegistry.Tests.Extensions
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Events;

    public class BuildingUnitAddressWasAttachedBuilder
    {
        private readonly Fixture _fixture;

        private int? _buildingPersistentLocalId;
        private int? _buildingUnitPersistentLocalId;
        private int? _addressPersistentLocalId;

        public BuildingUnitAddressWasAttachedBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public BuildingUnitAddressWasAttachedBuilder WithBuildingPersistentLocalId(int buildingPersistentLocalId)
        {
            _buildingPersistentLocalId = buildingPersistentLocalId;
            return this;
        }

        public BuildingUnitAddressWasAttachedBuilder WithBuildingUnitPersistentLocalId(int buildingUnitPersistentLocalId)
        {
            _buildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            return this;
        }

        public BuildingUnitAddressWasAttachedBuilder WithAddressPersistentLocalId(int addressPersistentLocalId)
        {
            _addressPersistentLocalId = addressPersistentLocalId;
            return this;
        }

        public BuildingUnitAddressWasAttachedV2 Build()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(_buildingPersistentLocalId ?? _fixture.Create<int>());
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(_buildingUnitPersistentLocalId ?? _fixture.Create<int>());
            var addressPersistentLocalId = new AddressPersistentLocalId(_addressPersistentLocalId ?? _fixture.Create<int>());

            var @event = new BuildingUnitAddressWasAttachedV2(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                addressPersistentLocalId);

            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            return @event;
        }
    }
}
