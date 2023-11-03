namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingNotRealization
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithStatusRealized_ThenBuildingWasCorrectToUnderConstruction()
        {
            var command = Fixture.Create<CorrectBuildingNotRealization>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasNotRealizedV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasCorrectedFromNotRealizedToPlanned(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithStatusPlanned_ThenDoNothing()
        {
            var command = Fixture.Create<CorrectBuildingNotRealization>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WhenBuildingIsRemoved_ThenThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<CorrectBuildingNotRealization>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.NotRealized)
                .WithBuildingGeometry(Fixture.Create<BuildingGeometry>())
                .WithIsRemoved()
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingIsRemovedException(command.BuildingPersistentLocalId)));
        }

        [Theory]
        [InlineData("UnderConstruction")]
        [InlineData("Realized")]
        [InlineData("Retired")]
        public void WithInvalidStatus_ThenThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<CorrectBuildingNotRealization>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Parse(status))
                .WithBuildingGeometry(Fixture.Create<BuildingGeometry>())
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Fact]
        public void WhenBuildingGeometryMethodIsMeasuredByGrb_ThenThrowsBuildingHasInvalidBuildingGeometryMethodException()
        {
            var command = Fixture.Create<CorrectBuildingNotRealization>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(Fixture.Create<BuildingPersistentLocalId>())
                .WithBuildingStatus(BuildingStatus.NotRealized)
                .WithBuildingGeometry(new BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.MeasuredByGrb))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidGeometryMethodException()));
        }

        [Fact]
        public void StateCheck()
        {
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(Fixture.Create<BuildingPersistentLocalId>())
                .WithBuildingStatus(BuildingStatus.NotRealized)
                .WithBuildingGeometry(new BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.Outlined))
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { buildingWasMigrated });

            // Act
            sut.CorrectNotRealizeConstruction();

            // Assert
            sut.BuildingStatus.Should().Be(BuildingStatus.Planned);
        }
    }
}
