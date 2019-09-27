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

        [Fact]
        public void Given_single_entry_last_index_is_duplicate_Then_should_not_be_added()
        {
            //real case: select * from crab.GebouweenheidMapping where TerreinobjectID = 11938486 order by[Index], MappingCreatedTimestamp

            var a = CreateBuildingUnitPersistentLocalIdAssignment(1, 1, 1, DateTime.UtcNow);
            var b = CreateBuildingUnitPersistentLocalIdAssignment(1, 2, 2, DateTime.UtcNow);
            var c = CreateBuildingUnitPersistentLocalIdAssignment(1, 3, 3, DateTime.UtcNow);
            var d = CreateBuildingUnitPersistentLocalIdAssignment(1, 4, 4, DateTime.UtcNow);
            var e = CreateBuildingUnitPersistentLocalIdAssignment(null, null, 5, DateTime.UtcNow);
            var f = CreateBuildingUnitPersistentLocalIdAssignment(1, null, 6, DateTime.UtcNow);
            var g = CreateBuildingUnitPersistentLocalIdAssignment(2, 1, 7, DateTime.UtcNow);
            var h = CreateBuildingUnitPersistentLocalIdAssignment(2, 2, 8, DateTime.UtcNow);
            var i = CreateBuildingUnitPersistentLocalIdAssignment(2, 3, 9, DateTime.UtcNow);
            var j = CreateBuildingUnitPersistentLocalIdAssignment(null, null, 10, DateTime.UtcNow);
            var k = CreateBuildingUnitPersistentLocalIdAssignment(3, 1, 11, DateTime.UtcNow.AddDays(1));
            var f2 = CreateBuildingUnitPersistentLocalIdAssignment(1, null, 11, DateTime.UtcNow.AddDays(2));
            var l = CreateBuildingUnitPersistentLocalIdAssignment(3, 2, 12, DateTime.UtcNow.AddDays(1));
            var k2 = CreateBuildingUnitPersistentLocalIdAssignment(3, 1, 12, DateTime.UtcNow.AddDays(2));
            var m = CreateBuildingUnitPersistentLocalIdAssignment(3, 3, 13, DateTime.UtcNow.AddDays(1));
            var l2 = CreateBuildingUnitPersistentLocalIdAssignment(3, 2, 13, DateTime.UtcNow.AddDays(2));
            var n = CreateBuildingUnitPersistentLocalIdAssignment(2, null, 14, DateTime.UtcNow.AddDays(1));
            var m2 = CreateBuildingUnitPersistentLocalIdAssignment(3, 3, 14, DateTime.UtcNow.AddDays(2));
            var o = CreateBuildingUnitPersistentLocalIdAssignment(4, 1, 15, DateTime.UtcNow.AddDays(1));
            var n2 = CreateBuildingUnitPersistentLocalIdAssignment(2, null, 15, DateTime.UtcNow.AddDays(2));
            var p = CreateBuildingUnitPersistentLocalIdAssignment(4, 2, 16, DateTime.UtcNow.AddDays(1));
            var o2 = CreateBuildingUnitPersistentLocalIdAssignment(4, 1, 16, DateTime.UtcNow.AddDays(2));
            var q = CreateBuildingUnitPersistentLocalIdAssignment(4, 3, 17, DateTime.UtcNow.AddDays(1));
            var p2 = CreateBuildingUnitPersistentLocalIdAssignment(4, 2, 17, DateTime.UtcNow.AddDays(2));
            var r = CreateBuildingUnitPersistentLocalIdAssignment(4, null, 18, DateTime.UtcNow.AddDays(1));
            var q2 = CreateBuildingUnitPersistentLocalIdAssignment(4, 3, 18, DateTime.UtcNow.AddDays(2));
            var r2 = CreateBuildingUnitPersistentLocalIdAssignment(4, null, 19, DateTime.UtcNow.AddDays(2));

            var testData = new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
            {
                a, b, c, d, e, f, g, h, i, j, k, f2, l, k2, m, l2, n, m2, o, n2, p, o2, q, p2, r, q2, r2
            };

            var expected = new List<AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId>
            {
                a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r
            };

            var sut = new DeduplicatedBuildingUnitPersistentLocalIdCollection(testData);
            sut.Should().BeEquivalentTo(expected);
        }

        private static AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId CreateBuildingUnitPersistentLocalIdAssignment(
            int? terrainObjectHouseNumberId,
            int? subaddressId,
            int index,
            DateTime assignmentDate)
        {
            return new AssignBuildingUnitPersistentLocalIdForCrabTerrainObjectId(
                terrainObjectHouseNumberId.HasValue ? new CrabTerrainObjectHouseNumberId(terrainObjectHouseNumberId.Value) : null,
                subaddressId.HasValue ? new CrabSubaddressId(subaddressId.Value) : null,
                index,
                new PersistentLocalId(1),
                new PersistentLocalIdAssignmentDate(Instant.FromDateTimeUtc(assignmentDate)));
        }
    }
}
