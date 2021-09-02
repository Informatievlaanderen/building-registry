namespace BuildingRegistry.Tests.Cases
{
    using System;
    using System.Collections.Generic;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using Building.Events;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;
    using WhenImportingCrabBuildingGeometry;
    using WhenImportingCrabTerrainObjectHouseNumber;
    using Xunit;
    using Xunit.Abstractions;

    public class CaseMultipleGeometries : SnapshotBasedTest
    {
        private ImportBuildingGeometryFromCrab? _buildingGeometryFromCrab;
        private ImportBuildingGeometryFromCrab? _retireBuildingGeometryFromCrab;
        private ImportBuildingGeometryFromCrab? _newBuildingGeometryFromCrab;

        public CaseMultipleGeometries(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture
                .Customize(new InfrastructureCustomization())
                .Customize(new WithNoDeleteModification())
                .Customize(new WithInfiniteLifetime())
                .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16));

            _ = new TestCase1AData(Fixture);
        }

        protected class TestCase1AData
        {
            public TestCase1AData(IFixture customizedFixture)
            {
                Gebouw1CrabTerrainObjectId = customizedFixture.Create<CrabTerrainObjectId>();
            }

            public CrabTerrainObjectId Gebouw1CrabTerrainObjectId { get; }

            public BuildingId Gebouw1Id => BuildingId.CreateFor(Gebouw1CrabTerrainObjectId);
            public LocalDateTime FromDateGeometry1 => LocalDateTime.FromDateTime(DateTime.Now);
            public LocalDateTime ToDateGeometry1 => LocalDateTime.FromDateTime(DateTime.Now.AddDays(1));
            public LocalDateTime FromDateGeometry2 => LocalDateTime.FromDateTime(DateTime.Now.AddDays(2));
        }

        protected TestCase1AData _ { get; }

        public IEventCentricTestSpecificationBuilder AddGeometry()
        {
            _buildingGeometryFromCrab = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithBuildingGeometryId(new CrabBuildingGeometryId(1))
                .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary()))
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb)
                .WithLifetime(new CrabLifetime(_.FromDateGeometry1, null))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now)));

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(_buildingGeometryFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingWasMeasuredByGrb(_.Gebouw1Id, ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPolygon.AsBinary())),
                    _buildingGeometryFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireGeometry()
        {
            _retireBuildingGeometryFromCrab = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithBuildingGeometryId(new CrabBuildingGeometryId(1))
                .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary()))
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb)
                .WithLifetime(new CrabLifetime(_.FromDateGeometry1, _.ToDateGeometry1))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(1)))); ;

            return new AutoFixtureScenario(Fixture)
                .Given(AddGeometry())
                .When(_retireBuildingGeometryFromCrab)
                .Then(_.Gebouw1Id,
                    _retireBuildingGeometryFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewGeometry()
        {
            _newBuildingGeometryFromCrab = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithBuildingGeometryId(new CrabBuildingGeometryId(2))
                .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygonWithNoValidPoints.AsBinary()))
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb)
                .WithLifetime(new CrabLifetime(_.FromDateGeometry2, null))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(2))));

            return new AutoFixtureScenario(Fixture)
                .Given(RetireGeometry())
                .When(_newBuildingGeometryFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingWasMeasuredByGrb(_.Gebouw1Id, ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPolygonWithNoValidPoints.AsBinary())),
                    _newBuildingGeometryFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder DeleteNewGeometry()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var buildingGeometryFromCrab = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithBuildingGeometryId(new CrabBuildingGeometryId(2))
                .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygonWithNoValidPoints.AsBinary()))
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb)
                .WithLifetime(new CrabLifetime(_.FromDateGeometry2, null))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(3))))
                .WithCrabModification(CrabModification.Delete);

            return new AutoFixtureScenario(Fixture)
                .Given(AddNewGeometry())
                .When(buildingGeometryFromCrab)
                .Then(
                    new Fact(_.Gebouw1Id, new BuildingWasMeasuredByGrb(_.Gebouw1Id, ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPolygon.AsBinary()))),
                    new Fact(_.Gebouw1Id, buildingGeometryFromCrab.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(_.Gebouw1Id), BuildingSnapshotBuilder.CreateDefaultSnapshot(_.Gebouw1Id)
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithGeometry(new BuildingGeometry(ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPolygon.AsBinary()), BuildingGeometryMethod.MeasuredByGrb))
                        .WithGeometryChronicle(new List<ImportBuildingGeometryFromCrab>
                        {
                            _buildingGeometryFromCrab,
                            _retireBuildingGeometryFromCrab,
                            _newBuildingGeometryFromCrab,
                            buildingGeometryFromCrab
                        })
                        .Build(7, EventSerializerSettings))
                );
        }

        [Fact]
        public void AddGeometryTest()
        {
            Assert(AddGeometry());
        }

        [Fact]
        public void RetireGeometryTest()
        {
            Assert(RetireGeometry());
        }

        [Fact]
        public void AddNewGeometryTest()
        {
            Assert(AddNewGeometry());
        }

        [Fact]
        public void DeleteNewGeometryTest()
        {
            Assert(DeleteNewGeometry());
        }
    }
}
