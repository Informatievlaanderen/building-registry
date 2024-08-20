namespace BuildingRegistry.Tests.AggregateTests.WhenNotRealizingBuilding
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using BuildingRegistry.Legacy;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void WithStatusPlanned_ThenBuildingWasNotRealized()
        {
            var command = Fixture.Create<NotRealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasNotRealizedV2(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithPlannedBuildingUnits_ThenBuildingUnitsWereNotRealized()
        {
            var command = Fixture.Create<NotRealizeBuilding>();

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttachedV2 = Fixture.Create<BuildingUnitAddressWasAttachedV2>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    buildingUnitWasPlannedV2,
                    buildingUnitAddressWasAttachedV2)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitAddressWasDetachedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId),
                            new AddressPersistentLocalId(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasNotRealizedV2(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithPlannedBuildingUnitsAndCommonPlanned_ThenBuildingUnitsWereNotRealized()
        {
            var command = Fixture.Create<NotRealizeBuilding>();

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttachedV2 = Fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitWasPlannedV2Two = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId + 1);
            var commonBuildingUnitWasPlanned = new BuildingUnitWasPlannedV2Builder(Fixture)
                .WithBuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId + 2)
                .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Common)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    buildingUnitWasPlannedV2,
                    buildingUnitAddressWasAttachedV2,
                    buildingUnitWasPlannedV2Two,
                    commonBuildingUnitWasPlanned)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitAddressWasDetachedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId),
                            new AddressPersistentLocalId(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2Two.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(commonBuildingUnitWasPlanned.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasNotRealizedV2(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithPlannedBuildingUnitsAndCommonRealized_ThenBuildingUnitsWereNotRealized()
        {
            var command = Fixture.Create<NotRealizeBuilding>();

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttachedV2 = Fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitWasPlannedV2Two = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId + 1);
            var commonBuildingUnitWasPlanned = new BuildingUnitWasPlannedV2Builder(Fixture)
                .WithBuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId + 2)
                .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Common)
                .Build();
            var commonBuildingUnitWasRealized = Fixture.Create<BuildingUnitWasRealizedV2>()
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(commonBuildingUnitWasPlanned.BuildingUnitPersistentLocalId));

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    buildingUnitWasPlannedV2,
                    buildingUnitAddressWasAttachedV2,
                    buildingUnitWasPlannedV2Two,
                    commonBuildingUnitWasPlanned,
                    commonBuildingUnitWasRealized)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitAddressWasDetachedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId),
                            new AddressPersistentLocalId(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2Two.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRetiredV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(commonBuildingUnitWasPlanned.BuildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasNotRealizedV2(command.BuildingPersistentLocalId))
                   )
            );
        }

        [Fact]
        public void WithStatusNotRealized_ThenDoNothing()
        {
            var command = Fixture.Create<NotRealizeBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasNotRealizedV2>())
                .When(command)
                .ThenNone());
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("Realized")]
        public void WithInvalidStatus_ThenThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<NotRealizeBuilding>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Parse(status))
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Fact]
        public void WithRemovedBuilding_ThenThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<NotRealizeBuilding>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithIsRemoved()
                .Build();

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
            var plannedBuildingUnitPersistentLocalId = new PersistentLocalId(123);
            var retiredBuildingUnitPersistentLocalId = new PersistentLocalId(456);
            var realizedBuildingUnitPersistentLocalId = new PersistentLocalId(789);
            var removedBuildingUnitPersistentLocalId = new PersistentLocalId(101);

            var plannedBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithPersistentLocalId(plannedBuildingUnitPersistentLocalId)
                .WithFunction(BuildingUnitFunction.Unknown)
                .WithStatus(BuildingUnitStatus.Planned)
                .WithAddress(5)
                .Build();

            var retiredBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithPersistentLocalId(retiredBuildingUnitPersistentLocalId)
                .WithFunction(BuildingUnitFunction.Unknown)
                .WithStatus(BuildingUnitStatus.Retired)
                .Build();

            var realizedBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithPersistentLocalId(realizedBuildingUnitPersistentLocalId)
                .WithFunction(BuildingUnitFunction.Unknown)
                .WithStatus(BuildingUnitStatus.Realized)
                .Build();

            var removedBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithPersistentLocalId(removedBuildingUnitPersistentLocalId)
                .WithStatus(BuildingUnitStatus.Planned)
                .WithIsRemoved()
                .Build();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.UnderConstruction)
                .WithBuildingUnit(plannedBuildingUnit)
                .WithBuildingUnit(retiredBuildingUnit)
                .WithBuildingUnit(realizedBuildingUnit)
                .WithBuildingUnit(removedBuildingUnit)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                buildingWasMigrated
            });

            // Act
            sut.NotRealizeConstruction();

            // Assert
            sut.BuildingStatus.Should().Be(BuildingStatus.NotRealized);

            var plannedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(plannedBuildingUnitPersistentLocalId));

            plannedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.NotRealized);
            plannedUnit.AddressPersistentLocalIds.Should().BeEmpty();

            var retiredUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(retiredBuildingUnitPersistentLocalId));

            retiredUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Retired);

            var realizedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(realizedBuildingUnitPersistentLocalId));

            realizedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Realized);

            var removedUnit = sut.BuildingUnits
                .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(removedBuildingUnitPersistentLocalId));

            removedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Planned);
        }
    }
}
