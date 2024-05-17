namespace BuildingRegistry.Tests.Extensions
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public class ReaddressAddressesBuilder
    {
        private readonly Fixture _fixture;
        private readonly Dictionary<BuildingUnitPersistentLocalId, List<ReaddressData>> _buildingUnitReaddresses = [];

        public ReaddressAddressesBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }
        
        public ReaddressAddressesBuilder WithReaddress(int buildingUnitId, int sourceAddressPersistentLocalId, int destinationAddressPersistentLocalId)
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(buildingUnitId);
            _buildingUnitReaddresses.TryAdd(buildingUnitPersistentLocalId, []);

            _buildingUnitReaddresses[buildingUnitPersistentLocalId].Add(new ReaddressData(
                new AddressPersistentLocalId(sourceAddressPersistentLocalId),
                new AddressPersistentLocalId(destinationAddressPersistentLocalId)));

            return this;
        }

        public ReaddressAddresses Build()
        {
            return new ReaddressAddresses(
                _fixture.Create<BuildingPersistentLocalId>(),
                _buildingUnitReaddresses
                    .ToDictionary(x => x.Key, x => (IReadOnlyList<ReaddressData>)x.Value),
                _fixture.Create<Provenance>());
        }
    }
}
