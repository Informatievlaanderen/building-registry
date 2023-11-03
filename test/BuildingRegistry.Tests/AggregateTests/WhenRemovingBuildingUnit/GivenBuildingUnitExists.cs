namespace BuildingRegistry.Tests.AggregateTests.WhenRemovingBuildingUnit
{
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingUnitExists : BuildingRegistryTest
    {
        public GivenBuildingUnitExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void ThenBuildingUnitWasRemoved()
        {
            var command = Fixture.Create<RemoveBuildingUnit>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasRemovedV2(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithAttachedAddresses_ThenBuildingUnitWasRemovedAndAddressesWereDetached()
        {
            var command = Fixture.Create<RemoveBuildingUnit>();

            var buildingUnitAddressWasAttachedV2 = new BuildingUnitAddressWasAttachedBuilder(Fixture)
                .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                .WithAddressPersistentLocalId(123)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>(),
                    buildingUnitAddressWasAttachedV2)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitAddressWasDetachedV2(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId,
                            new AddressPersistentLocalId(123))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRemovedV2(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithRemovedBuildingUnit_ThenDoNothing()
        {
            var command = Fixture.Create<RemoveBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithStatus(BuildingUnitStatus.Planned)
                    .WithFunction(BuildingUnitFunction.Unknown)
                    .WithIsRemoved()
                    .Build())
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithCommonBuilding_ThenThrowsBuildingUnitHasInvalidFunctionException()
        {
            var command = Fixture.Create<RemoveBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingUnit(new BuildingUnitBuilder(Fixture)
                    .WithStatus(BuildingUnitStatus.Planned)
                    .WithFunction(BuildingUnitFunction.Common)
                    .Build())
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidFunctionException()));
        }

        [Fact]
        public void WithPlannedCommonBuildingUnitAndTwoOtherBuildingUnits_ThenCommonBuildingUnitWasNotRealized()
        {
            var command = new RemoveBuildingUnit(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(1),
                Fixture.Create<Provenance>());

            var commonBuildingUnitWasAddedV2 = Fixture.Create<CommonBuildingUnitWasAddedV2>()
                .WithBuildingUnitStatus(BuildingRegistry.Building.BuildingUnitStatus.Planned)
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(3));

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                        .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Unknown),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(2))
                        .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Unknown),
                    commonBuildingUnitWasAddedV2)
                .When(command)
                .Then(
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRemovedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId)))));
        }

        [Fact]
        public void WithRealizedCommonBuildingUnitAndTwoOtherBuildingUnits_ThenCommonBuildingUnitWasRetired()
        {
            var command = new RemoveBuildingUnit(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(1),
                Fixture.Create<Provenance>());

            var commonBuildingUnitWasAddedV2 = Fixture.Create<CommonBuildingUnitWasAddedV2>()
                .WithBuildingUnitStatus(BuildingRegistry.Building.BuildingUnitStatus.Realized)
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(3));

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                        .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Unknown),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(2))
                        .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Unknown),
                    commonBuildingUnitWasAddedV2)
                .When(command)
                .Then(
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRemovedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRetiredV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId)))));
        }

        [Fact]
        public void WithAllBuildingUnitsRemoved_ThenCommonBuildingUnitIsAlsoRemoved()
        {
            var command = new RemoveBuildingUnit(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(1),
                Fixture.Create<Provenance>());

            var commonBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(3);
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    command.BuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    new BuildingUnitPersistentLocalId(2),
                    BuildingUnitFunction.Unknown,
                    isRemoved: true)
                .WithBuildingUnit(
                    BuildingUnitStatus.NotRealized,
                    commonBuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Common)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRemovedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRemovedV2(
                            command.BuildingPersistentLocalId,
                            commonBuildingUnitPersistentLocalId))));
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("Realized")]
        public void WithActiveCommonBuildingUnitAndThreeOtherBuildingUnits_ThenNothingForCommonBuildingUnit(string buildingUnitStatus)
        {
            var command = new RemoveBuildingUnit(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(1),
                Fixture.Create<Provenance>());

            var commonBuildingUnitWasAddedV2 = Fixture.Create<CommonBuildingUnitWasAddedV2>()
                .WithBuildingUnitStatus(BuildingRegistry.Building.BuildingUnitStatus.Parse(buildingUnitStatus))
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(4));

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Unknown)
                        .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(2))
                        .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Unknown),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(3))
                        .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Unknown),
                    commonBuildingUnitWasAddedV2)
                .When(command)
                .Then(
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRemovedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void StateCheck()
        {
            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(1));
            var secondBuildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(2));
            var commonBuildingUnitWasAdded = Fixture.Create<CommonBuildingUnitWasAddedV2>()
                .WithBuildingUnitStatus(BuildingRegistry.Building.BuildingUnitStatus.Realized)
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(3));

            var buildingUnitAddressWasAttachedV2 = new BuildingUnitAddressWasAttachedBuilder(Fixture)
                .WithBuildingUnitPersistentLocalId(1)
                .WithAddressPersistentLocalId(123)
                .Build();

            var buildingUnitAddressWasDetachedV2 = new BuildingUnitAddressWasDetachedBuilder(Fixture)
                .WithBuildingUnitPersistentLocalId(1)
                .WithAddressPersistentLocalId(123)
                .Build();

            var buildingUnitWasRemoved = Fixture.Create<BuildingUnitWasRemovedV2>()
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(1));

            // Act
            building.Initialize(new object[]
            {
                buildingWasPlanned,
                buildingUnitWasPlanned,
                secondBuildingUnitWasPlanned,
                commonBuildingUnitWasAdded,
                buildingUnitAddressWasAttachedV2,
                buildingUnitAddressWasDetachedV2,
                buildingUnitWasRemoved
            });

            // Assert
            building.BuildingUnits.Should().NotBeEmpty();
            building.BuildingUnits.Count.Should().Be(3);

            var buildingUnit = building.BuildingUnits
                .Single(x => x.BuildingUnitPersistentLocalId == buildingUnitWasPlanned.BuildingUnitPersistentLocalId);
            buildingUnit.IsRemoved.Should().BeTrue();
            buildingUnit.AddressPersistentLocalIds.Should().BeEmpty();

            var commonBuildingUnit = building.BuildingUnits
                .Single(x => x.BuildingUnitPersistentLocalId == commonBuildingUnitWasAdded.BuildingUnitPersistentLocalId);
            commonBuildingUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Realized);

            building.LastEventHash.Should().Be(buildingUnitWasRemoved.GetHash());
        }
    }
}
