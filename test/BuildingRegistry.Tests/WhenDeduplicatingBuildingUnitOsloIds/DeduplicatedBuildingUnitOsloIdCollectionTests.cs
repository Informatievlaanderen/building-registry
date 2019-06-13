namespace BuildingRegistry.Tests.WhenDeduplicatingBuildingUnitOsloIds
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building;
    using FluentAssertions;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using ValueObjects;
    using Xunit;

    public class DeduplicatedBuildingUnitOsloIdCollectionTests
    {
        [Fact]
        public void Given_more_than_2_units_on_same_index_Then_InvalidOperationException_is_thrown()
        {
            var testData = new List<AssignBuildingUnitOsloIdForCrabTerrainObjectId>
            {
                CreateBuildingUnitOlsoIdAssignment(1, 1, 1, DateTime.UtcNow),
                CreateBuildingUnitOlsoIdAssignment(2, 5, 1, DateTime.UtcNow),
                CreateBuildingUnitOlsoIdAssignment(3, 4, 1, DateTime.UtcNow)
            };

            Assert.Throws<InvalidOperationException>(() => new DeduplicatedBuildingUnitOsloIdCollection(testData));
        }

        [Fact]
        public void Given_1_unit_Then_AssignBuildingUnitOsloId_was_added()
        {
            var testData = new List<AssignBuildingUnitOsloIdForCrabTerrainObjectId>
            {
                CreateBuildingUnitOlsoIdAssignment(1, 1, 1, DateTime.UtcNow)
            };

            var sut = new DeduplicatedBuildingUnitOsloIdCollection(testData);
            sut.Should().BeEquivalentTo(testData);
        }

        [Fact]
        public void Given_1_unit_on_different_indexes_Then_AssignBuildingUnitOsloId_was_added()
        {
            var testData = new List<AssignBuildingUnitOsloIdForCrabTerrainObjectId>
            {
                CreateBuildingUnitOlsoIdAssignment(1, 1, 1, DateTime.UtcNow),
                CreateBuildingUnitOlsoIdAssignment(1, 1, 2, DateTime.UtcNow),
                CreateBuildingUnitOlsoIdAssignment(1, 1, 3, DateTime.UtcNow)
            };

            var sut = new DeduplicatedBuildingUnitOsloIdCollection(testData);
            sut.Should().BeEquivalentTo(testData);
        }

        [Fact]
        public void Given_1_unit_on_different_indexes_and_index_starts_at_2_Then_AssignBuildingUnitOsloId_was_added()
        {
            var testData = new List<AssignBuildingUnitOsloIdForCrabTerrainObjectId>
            {
                CreateBuildingUnitOlsoIdAssignment(1, 1, 1, DateTime.UtcNow),
                CreateBuildingUnitOlsoIdAssignment(1, 1, 2, DateTime.UtcNow),
                CreateBuildingUnitOlsoIdAssignment(1, 1, 3, DateTime.UtcNow)
            };

            var sut = new DeduplicatedBuildingUnitOsloIdCollection(testData);
            sut.Should().BeEquivalentTo(testData);
        }

        [Fact]
        public void Given_1_unit_on_same_index_Then_AssignBuildingUnitOsloId_was_added()
        {
            var a1 = CreateBuildingUnitOlsoIdAssignment(1, 1, 1, DateTime.UtcNow.AddDays(-2));
            var b2 = CreateBuildingUnitOlsoIdAssignment(1, 2, 2, DateTime.UtcNow.AddDays(-1));
            var c1 = CreateBuildingUnitOlsoIdAssignment(1, 3, 1, DateTime.UtcNow);
            var a3 = CreateBuildingUnitOlsoIdAssignment(1, 1, 3, DateTime.UtcNow);
            var a4 = CreateBuildingUnitOlsoIdAssignment(1, 1, 4, DateTime.UtcNow.AddDays(1));

            var testData = new List<AssignBuildingUnitOsloIdForCrabTerrainObjectId>
            {
                a1,
                b2,
                c1,
                a3,
                a4
            };

            var expected = new List<AssignBuildingUnitOsloIdForCrabTerrainObjectId>
            {
                a1,
                b2,
                c1,
                a4,
            };

            var sut = new DeduplicatedBuildingUnitOsloIdCollection(testData);
            sut.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Given_2_units_on_same_index_Then_AssignBuildingUnitOsloId_was_added()
        {
            var a1 = CreateBuildingUnitOlsoIdAssignment(1, 1, 1, DateTime.UtcNow.AddDays(-2));
            var b2 = CreateBuildingUnitOlsoIdAssignment(1, 2, 2, DateTime.UtcNow.AddDays(-1));
            var c1 = CreateBuildingUnitOlsoIdAssignment(1, 3, 1, DateTime.UtcNow);
            var b3 = CreateBuildingUnitOlsoIdAssignment(1, 2, 3, DateTime.UtcNow);
            var a2 = CreateBuildingUnitOlsoIdAssignment(1, 1, 2, DateTime.UtcNow);
            var a4 = CreateBuildingUnitOlsoIdAssignment(1, 1, 4, DateTime.UtcNow.AddDays(1));

            var testData = new List<AssignBuildingUnitOsloIdForCrabTerrainObjectId>
            {
                a1,
                b2,
                c1,
                a2,
                b3,
                a4
            };

            var expected = new List<AssignBuildingUnitOsloIdForCrabTerrainObjectId>
            {
                a1,
                b2,
                c1,
                a4
            };

            var sut = new DeduplicatedBuildingUnitOsloIdCollection(testData);
            sut.Should().BeEquivalentTo(expected);
        }

        private static AssignBuildingUnitOsloIdForCrabTerrainObjectId CreateBuildingUnitOlsoIdAssignment(
            int terrainObjectHouseNumberId,
            int subaddressId,
            int index,
            DateTime assignmentDate)
        {
            return new AssignBuildingUnitOsloIdForCrabTerrainObjectId(
                new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId),
                new CrabSubaddressId(subaddressId),
                index,
                new OsloId(1),
                new OsloAssignmentDate(Instant.FromDateTimeUtc(assignmentDate)));
        }
    }
}
