namespace BuildingRegistry.Tests.SnapshotVerifier
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.SnapshotVerifier;
    using Building;
    using FluentAssertions;
    using KellermanSoftware.CompareNetObjects;
    using Xunit;

    public class CompareAddressPersistentLocalIdsTests
    {
        private readonly List<AddressPersistentLocalId> _addressPersistentLocalIds1;

        private readonly CompareLogic _compareLogic;

        public CompareAddressPersistentLocalIdsTests()
        {
            _addressPersistentLocalIds1 = new List<AddressPersistentLocalId>
            {
                new(1),
                new(2),
            };

            var config = DefaultComparisonConfig.Instance;
            config.MaxDifferences = 100;

            _compareLogic = new CompareLogic(config);
        }

        [Fact]
        public void AreEqual()
        {
            var addressPersistentLocalIds2 = new List<AddressPersistentLocalId>
            {
                new(1),
                new(2)
            };

            var result = _compareLogic.Compare(_addressPersistentLocalIds1, addressPersistentLocalIds2);

            result.AreEqual.Should().BeTrue();
        }

        [Fact]
        public void WithDifferentOrder_AreEqual()
        {
            var addressPersistentLocalIds2 = new List<AddressPersistentLocalId>
            {
                new(2),
                new(1)
            };

            var result = _compareLogic.Compare(_addressPersistentLocalIds1, addressPersistentLocalIds2);

            result.AreEqual.Should().BeTrue();
        }

        [Fact]
        public void SameCountButDifferentItems()
        {
            var addressPersistentLocalIds2 = new List<AddressPersistentLocalId>
            {
                new(3),
                new(4)
            };

            var result = _compareLogic.Compare(_addressPersistentLocalIds1, addressPersistentLocalIds2);

            result.AreEqual.Should().BeFalse();
        }

        [Fact]
        public void DifferentCount()
        {
            var addressPersistentLocalIds2 = new List<AddressPersistentLocalId>
            {
                new(1)
            };

            var result = _compareLogic.Compare(_addressPersistentLocalIds1, addressPersistentLocalIds2);

            result.AreEqual.Should().BeFalse();
            result.DifferencesString.Should().Contain("[List`1.Count,List`1.Count]");
        }
    }
}
