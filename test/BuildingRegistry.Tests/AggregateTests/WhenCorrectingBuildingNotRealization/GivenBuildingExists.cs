namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingNotRealization
{
    using System.Collections.Generic;
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
    using Moq;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingId = Building.BuildingId;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnit = Building.Commands.BuildingUnit;

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
        public void WhenBuildingIsRemoved_ThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<CorrectBuildingNotRealization>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.NotRealized,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: true,
                new List<BuildingUnit>()
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

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
        public void WithInvalidStatus_ThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<CorrectBuildingNotRealization>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Parse(status),
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>()
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Fact]
        public void WhenBuildingGeometryMethodIsMeasuredByGrb_ThrowsBuildingHasInvalidBuildingGeometryMethodException()
        {
            var command = Fixture.Create<CorrectBuildingNotRealization>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.NotRealized,
                new BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.MeasuredByGrb),
                isRemoved: false,
                new List<BuildingUnit>()
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidBuildingGeometryMethodException()));
        }

        [Fact]
        public void StateCheck()
        {
            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.NotRealized,
                new BuildingGeometry(
                    Fixture.Create<ExtendedWkbGeometry>(),
                    BuildingGeometryMethod.Outlined),
                isRemoved: false,
                new List<BuildingUnit>());
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()).Create();
            sut.Initialize(new List<object> { buildingWasMigrated });

            // Act
            sut.CorrectNotRealizeConstruction();

            // Assert
            sut.BuildingStatus.Should().Be(BuildingStatus.Planned);
        }
    }
}
