namespace BuildingRegistry.Tests.AggregateTests.WhenPlacingBuildingUnderConstruction
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
    using BuildingUnit = Building.Commands.BuildingUnit;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithStatusPlanned_ThenBuildingBecameUnderConstruction()
        {
            var command = Fixture.Create<PlaceBuildingUnderConstruction>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingBecameUnderConstructionV2(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithStatusUnderConstruction_ThenNone()
        {
            var command = Fixture.Create<PlaceBuildingUnderConstruction>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingBecameUnderConstructionV2>())
                .When(command)
                .ThenNone());
        }

        [Theory]
        [InlineData("Realized")]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void WithInvalidStatus_ThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<PlaceBuildingUnderConstruction>();

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
        public void BuildingIsRemoved_ThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<PlaceBuildingUnderConstruction>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Planned,
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


        [Fact]
        public void StateCheck()
        {
            var sut = new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()).Create();
            sut.Initialize(new List<object>{ Fixture.Create<BuildingWasPlannedV2>() });
            sut.PlaceUnderConstruction();
            sut.BuildingStatus.Should().Be(BuildingStatus.UnderConstruction);
        }
    }
}
