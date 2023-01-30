namespace BuildingRegistry.Tests.Legacy.Cases
{
    using System;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Crab;
    using BuildingRegistry.Legacy.Events;
    using Fixtures;
    using NodaTime;
    using WhenImportingCrabBuildingGeometry;
    using Xunit;
    using Xunit.Abstractions;

    public class CaseMultipleGeometries : AutofacBasedTest
    {
        public CaseMultipleGeometries(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture = new Fixture()
                    .Customize(new InfrastructureCustomization())
                    .Customize(new WithNoDeleteModification())
                    .Customize(new WithInfiniteLifetime())
                    .Customize(new WithFixedBuildingUnitIdFromHouseNumber(1, 16))
                ;

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

        protected readonly IFixture Fixture;
        protected TestCase1AData _ { get; }

        public IEventCentricTestSpecificationBuilder AddGeometry()
        {
            var buildingGeometryFromCrab = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithBuildingGeometryId(new CrabBuildingGeometryId(1))
                .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary()))
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb)
                .WithLifetime(new CrabLifetime(_.FromDateGeometry1, null))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now)));

            return new AutoFixtureScenario(Fixture)
                .Given<BuildingWasRegistered>(_.Gebouw1Id)
                .When(buildingGeometryFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingWasMeasuredByGrb(_.Gebouw1Id, ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPolygon.AsBinary())),
                    buildingGeometryFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder RetireGeometry()
        {
            var buildingGeometryFromCrab = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithBuildingGeometryId(new CrabBuildingGeometryId(1))
                .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary()))
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb)
                .WithLifetime(new CrabLifetime(_.FromDateGeometry1, _.ToDateGeometry1))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(1)))); ;

            return new AutoFixtureScenario(Fixture)
                .Given(AddGeometry())
                .When(buildingGeometryFromCrab)
                .Then(_.Gebouw1Id,
                    buildingGeometryFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder AddNewGeometry()
        {
            var buildingGeometryFromCrab = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithBuildingGeometryId(new CrabBuildingGeometryId(2))
                .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygonWithNoValidPoints.AsBinary()))
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb)
                .WithLifetime(new CrabLifetime(_.FromDateGeometry2, null))
                .WithTimestamp(new CrabTimestamp(Instant.FromDateTimeOffset(DateTimeOffset.Now.AddDays(2))));

            return new AutoFixtureScenario(Fixture)
                .Given(RetireGeometry())
                .When(buildingGeometryFromCrab)
                .Then(_.Gebouw1Id,
                    new BuildingWasMeasuredByGrb(_.Gebouw1Id, ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPolygonWithNoValidPoints.AsBinary())),
                    buildingGeometryFromCrab.ToLegacyEvent());
        }

        public IEventCentricTestSpecificationBuilder DeleteNewGeometry()
        {
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
                .Then(_.Gebouw1Id,
                    new BuildingWasMeasuredByGrb(_.Gebouw1Id, ExtendedWkbGeometry.CreateEWkb(GeometryHelper.ValidPolygon.AsBinary())),
                    buildingGeometryFromCrab.ToLegacyEvent());
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
