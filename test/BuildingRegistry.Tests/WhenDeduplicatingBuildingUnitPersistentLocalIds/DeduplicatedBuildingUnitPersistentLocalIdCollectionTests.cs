namespace BuildingRegistry.Tests.WhenDeduplicatingBuildingUnitPersistentLocalIds
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building;
    using FluentAssertions;
    using NodaTime;
    using ValueObjects;
    using Xunit;

    public class DeduplicatedBuildingUnitPersistentLocalIdCollectionTests
    {
        [Fact]
        public void Given_more_than_2_units_on_same_index_Then_InvalidOperationException_is_thrown()
        {
            var testData = new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
            {
                CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 1, DateTime.UtcNow),
                CreateBuildingUnitPersistentLocalIdAssignment(2, 5, 1, DateTime.UtcNow),
                CreateBuildingUnitPersistentLocalIdAssignment(3, 4, 1, DateTime.UtcNow)
            };

            Assert.Throws<InvalidOperationException>(() => new DeduplicatedBuildingUnitPersistentLocalIdCollection(testData));
        }

        [Fact]
        public void Given_1_unit_Then_AssignBuildingUnitPersistentLocalId_was_added()
        {
            var testData = new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
            {
                CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 1, DateTime.UtcNow)
            };

            var sut = new DeduplicatedBuildingUnitPersistentLocalIdCollection(testData);
            sut.Should().BeEquivalentTo(testData);
        }

        [Fact]
        public void Given_1_unit_on_different_indexes_Then_AssignBuildingUnitPersistentLocalId_was_added()
        {
            var testData = new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
            {
                CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 1, DateTime.UtcNow),
                CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 2, DateTime.UtcNow),
                CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 3, DateTime.UtcNow)
            };

            var sut = new DeduplicatedBuildingUnitPersistentLocalIdCollection(testData);
            sut.Should().BeEquivalentTo(testData);
        }

        [Fact]
        public void Given_1_unit_on_different_indexes_and_index_starts_at_2_Then_AssignBuildingUnitPersistentLocalId_was_added()
        {
            var testData = new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
            {
                CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 1, DateTime.UtcNow),
                CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 2, DateTime.UtcNow),
                CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 3, DateTime.UtcNow)
            };

            var sut = new DeduplicatedBuildingUnitPersistentLocalIdCollection(testData);
            sut.Should().BeEquivalentTo(testData);
        }

        [Fact]
        public void Given_1_unit_on_same_index_Then_AssignBuildingUnitPersistentLocalId_was_added()
        {
            var a1 = CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 1, DateTime.UtcNow.AddDays(-2));
            var b2 = CreateBuildingUnitPersistentLocalIdAssignment(1, 2, 2, DateTime.UtcNow.AddDays(-1));
            var c1 = CreateBuildingUnitPersistentLocalIdAssignment(1, 3, 1, DateTime.UtcNow);
            var a3 = CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 3, DateTime.UtcNow);
            var a4 = CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 4, DateTime.UtcNow.AddDays(1));

            var testData = new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
            {
                a1,
                b2,
                c1,
                a3,
                a4
            };

            var expected = new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
            {
                a1,
                b2,
                c1,
                a4,
            };

            var sut = new DeduplicatedBuildingUnitPersistentLocalIdCollection(testData);
            sut.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Given_2_units_on_same_index_Then_AssignBuildingUnitPersistentLocalId_was_added()
        {
            var a1 = CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 1, DateTime.UtcNow.AddDays(-2));
            var b2 = CreateBuildingUnitPersistentLocalIdAssignment(1, 2, 2, DateTime.UtcNow.AddDays(-1));
            var c1 = CreateBuildingUnitPersistentLocalIdAssignment(1, 3, 1, DateTime.UtcNow);
            var b3 = CreateBuildingUnitPersistentLocalIdAssignment(1, 2, 3, DateTime.UtcNow);
            var a2 = CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 2, DateTime.UtcNow);
            var a4 = CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 4, DateTime.UtcNow.AddDays(1));

            var testData = new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
            {
                a1,
                b2,
                c1,
                a2,
                b3,
                a4
            };

            var expected = new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
            {
                a1,
                b2,
                c1,
                a4
            };

            var sut = new DeduplicatedBuildingUnitPersistentLocalIdCollection(testData);
            sut.Should().BeEquivalentTo(expected);
        }

        private static AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId CreateBuildingUnitPersistentLocalIdAssignment(
            int terrainObjectHouseNumberId,
            int subaddressId,
            int index,
            DateTime assignmentDate)
        {
            return new AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId(
                new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId),
                new CrabSubaddressId(subaddressId),
                index,
                new PersistentLocalId(1),
                new PersistentLocalIdAssignmentDate(Instant.FromDateTimeUtc(assignmentDate)));
        }
    }
}
