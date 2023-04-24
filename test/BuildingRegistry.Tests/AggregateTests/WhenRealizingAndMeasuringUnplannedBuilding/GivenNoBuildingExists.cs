namespace BuildingRegistry.Tests.AggregateTests.WhenRealizingAndMeasuringUnplannedBuilding
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNoBuildingExists : BuildingRegistryTest
    {
        public GivenNoBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void ThenUnplannedBuildingWasRealizedAndMeasured()
        {
            var command = Fixture.Create<RealizeAndMeasureUnplannedBuilding>();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new UnplannedBuildingWasRealizedAndMeasured(
                        command.BuildingPersistentLocalId,
                        command.Geometry)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingGeometryWasImportedFromGrb(command.BuildingPersistentLocalId,
                            command.BuildingGrbData))));
        }

        [Fact]
        public void WithPointAsGeometry_ThenInvalidPolygonExceptionWasThrown()
        {
            Fixture.Customize(new WithValidPoint());
            var command = Fixture.Create<RealizeAndMeasureUnplannedBuilding>();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Throws(new PolygonIsInvalidException()));
        }

        [Fact]
        public void ThenBuildingStateWasCorrectlySet()
        {
            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();

            var unplannedBuildingWasRealizedAndMeasured = Fixture.Create<UnplannedBuildingWasRealizedAndMeasured>();
            ((ISetProvenance)unplannedBuildingWasRealizedAndMeasured).SetProvenance(Fixture.Create<Provenance>());

            // Act
            building.Initialize(new object[]
            {
                unplannedBuildingWasRealizedAndMeasured
            });

            // Assert
            building.BuildingPersistentLocalId.Should().Be(new BuildingPersistentLocalId(unplannedBuildingWasRealizedAndMeasured.BuildingPersistentLocalId));
            building.BuildingStatus.Should().Be(BuildingStatus.Realized);
            building.BuildingGeometry.Geometry.Should().Be(new ExtendedWkbGeometry(unplannedBuildingWasRealizedAndMeasured.ExtendedWkbGeometry));
            building.BuildingGeometry.Method.Should().Be(BuildingGeometryMethod.MeasuredByGrb);
        }
    }
}
