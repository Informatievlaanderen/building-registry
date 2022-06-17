namespace BuildingRegistry.Tests.AggregateTests.WhenRealizingBuilding
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
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
        public void WithStatusUnderConstruction_ThenBuildingWasRealized()
        {
            var command = Fixture.Create<RealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingBecameUnderConstructionV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasRealizedV2(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithStatusRealized_ThenDoNothing()
        {
            var command = Fixture.Create<RealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingBecameUnderConstructionV2>(),
                    Fixture.Create<BuildingWasRealizedV2>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithStatusPlanned_ThrowsBuildingCannotBeRealizedException()
        {
            var command = Fixture.Create<RealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>())
                .When(command)
                .Throws(new BuildingCannotBeRealizedException(command.BuildingPersistentLocalId)));
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void WithInvalidStatus_ThrowsBuildingCannotBeRealizedException(string status)
        {
            Fixture.Register(() => BuildingStatus.Parse(status));

            var command = Fixture.Create<RealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasMigrated>())
                .When(command)
                .Throws(new BuildingCannotBeRealizedException(command.BuildingPersistentLocalId)));
        }

        [Fact]
        public void StateCheck()
        {
            Fixture.Register(() => BuildingStatus.UnderConstruction);
            var sut = Building.Factory();
            sut.Initialize(new List<object> { Fixture.Create<BuildingWasMigrated>() });
            sut.RealizeConstruction();
            sut.BuildingStatus.Should().Be(BuildingStatus.Realized);
        }
    }
}
