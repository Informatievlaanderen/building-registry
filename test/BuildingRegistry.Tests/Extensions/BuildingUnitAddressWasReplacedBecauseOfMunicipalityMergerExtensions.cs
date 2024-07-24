namespace BuildingRegistry.Tests.Extensions
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Events;

    public class BuildingUnitAddressWasReplacedBecauseOfMunicipalityMergerBuilder
    {
        private readonly Fixture _fixture;

        private BuildingPersistentLocalId? _buildingPersistentLocalId;
        private BuildingUnitPersistentLocalId? _buildingUnitPersistentLocalId;
        private AddressPersistentLocalId? _newAddressPersistentLocalId;
        private AddressPersistentLocalId? _previousAddressPersistentLocalId;

        public BuildingUnitAddressWasReplacedBecauseOfMunicipalityMergerBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public BuildingUnitAddressWasReplacedBecauseOfMunicipalityMergerBuilder WithBuildingPersistentLocalId(int buildingPersistentLocalId)
        {
            _buildingPersistentLocalId = new BuildingPersistentLocalId(buildingPersistentLocalId);
            return this;
        }

        public BuildingUnitAddressWasReplacedBecauseOfMunicipalityMergerBuilder WithBuildingUnitPersistentLocalId(int buildingUnitPersistentLocalId)
        {
            _buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            return this;
        }

        public BuildingUnitAddressWasReplacedBecauseOfMunicipalityMergerBuilder WithNewAddressPersistentLocalId(int addressPersistentLocalId)
        {
            _newAddressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId);
            return this;
        }

        public BuildingUnitAddressWasReplacedBecauseOfMunicipalityMergerBuilder WithPreviousAddressPersistentLocalId(int addressPersistentLocalId)
        {
            _previousAddressPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId);
            return this;
        }

        public BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger Build()
        {
            var buildingPersistentLocalId = _buildingPersistentLocalId ?? _fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = _buildingUnitPersistentLocalId ?? _fixture.Create<BuildingUnitPersistentLocalId>();
            var previousAddressPersistentLocalId = _previousAddressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>();
            var newAddressPersistentLocalId = _newAddressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>();

            var @event = new BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                newAddressPersistentLocalId,
                previousAddressPersistentLocalId);

            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            return @event;
        }
    }
}
