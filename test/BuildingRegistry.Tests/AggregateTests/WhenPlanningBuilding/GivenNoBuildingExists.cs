namespace BuildingRegistry.Tests.AggregateTests.WhenPlanningBuilding
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNoBuildingExists : BuildingRegistryTest
    {
        public GivenNoBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {

        }

        [Fact]
        public void ThenBuildingWasPlanned()
        {
            var command = Fixture.Create<PlanBuilding>();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasPlannedV2(
                        command.BuildingPersistentLocalId,
                        command.Geometry))));
        }

        [Fact]
        public void WithPointAsGeometry_ThenInvalidPolygonExceptionWasThrown()
        {
            Fixture.Customize(new WithValidPoint());
            var command = Fixture.Create<PlanBuilding>();

            Assert(new Scenario()
                .GivenNone()
                .When(command)
                .Throws(new PolygonIsInvalidException()));
        }

        [Fact]
        public void ThenBuildingStateWasCorrectlySet()
        {
            var command = Fixture.Create<PlanBuilding>();

            // Act
            var result = Building.Plan(
                new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()),
                command.BuildingPersistentLocalId,
                command.Geometry);

            // Assert
            result.Should().NotBeNull();
            result.BuildingPersistentLocalId.Should().Be(command.BuildingPersistentLocalId);
            result.BuildingStatus.Should().Be(BuildingStatus.Planned);
            result.BuildingGeometry.Geometry.Should().Be(command.Geometry);
            result.BuildingGeometry.Method.Should().Be(BuildingGeometryMethod.Outlined);
        }
    }
}
