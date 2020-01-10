namespace BuildingRegistry.Tests.WhenImportingCrabBuildingGeometry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Building.Commands.Crab;
    using Building.Events;
    using ValueObjects;
    using ValueObjects.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuilding_GeometryCorrections : AutofacBasedTest
    {
        private readonly Fixture _fixture;

        public GivenBuilding_GeometryCorrections(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithInfiniteLifetime());
            _fixture.Customize(new WithValidPolygon());
        }

        [Fact]
        public void WithMethodIsOutlinedAndIsCorrection()
        {
            var importGeometry = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Outlined)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importGeometry)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingOutlineWasCorrected(_fixture.Create<BuildingId>(), GeometryHelper.CreateEwkbFrom(importGeometry.BuildingGeometry)),
                    importGeometry.ToLegacyEvent()));
        }

        [Fact]
        public void WithMethodIsSurveyAndIsCorrection()
        {
            var importGeometry = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Survey)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importGeometry)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingOutlineWasCorrected(_fixture.Create<BuildingId>(), GeometryHelper.CreateEwkbFrom(importGeometry.BuildingGeometry)),
                    importGeometry.ToLegacyEvent()));
        }

        [Fact]
        public void WithMethodIsGrbAndIsCorrection()
        {
            var importGeometry = _fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importGeometry)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingMeasurementByGrbWasCorrected(_fixture.Create<BuildingId>(), GeometryHelper.CreateEwkbFrom(importGeometry.BuildingGeometry)),
                    importGeometry.ToLegacyEvent()));
        }
    }
}
