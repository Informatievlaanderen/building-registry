namespace BuildingRegistry.Tests.AggregateTests.WhenPlacingBuildingUnderConstruction
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
        public void WithStatusUnderConstruction_ThrowsBuildingCannotBePlacedUnderConstructionException()
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
        public void WithInvalidStatus_ThrowsBuildingCannotBePlacedUnderConstructionException(string status)
        {
            Fixture.Register(() => BuildingStatus.Parse(status));

            var command = Fixture.Create<PlaceBuildingUnderConstruction>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasMigrated>())
                .When(command)
                .Throws(new BuildingCannotBePlacedUnderConstructionException(command.BuildingPersistentLocalId)));
        }

        [Fact]
        public void StateCheck()
        {
            var sut = Building.Factory();
            sut.Initialize(new List<object>{ Fixture.Create<BuildingWasPlannedV2>() });
            sut.PlaceUnderConstruction();
            sut.BuildingStatus.Should().Be(BuildingStatus.UnderConstruction);
        }
    }
}
