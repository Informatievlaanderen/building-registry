namespace BuildingRegistry.Tests.AggregateTests.WhenNotRealizingBuildingUnit
{
    using System;
    using System.Collections.Generic;
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
    using Moq;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnit = Building.Commands.BuildingUnit;

    public class GivenBuildingUnitExists: BuildingRegistryTest
    {
        public GivenBuildingUnitExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void ThenBuildingUnitWasNotRealized()
        {
            var command = Fixture.Create<NotRealizeBuildingUnit>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasNotRealizedV2(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithAttachedAddresses_ThenBuildingUnitWasNotRealizedAndAddressesDetached()
        {
            var command = Fixture.Create<NotRealizeBuildingUnit>();

            var buildingUnitAddressWasAttachedV2 = new BuildingUnitAddressWasAttachedV2(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                new AddressPersistentLocalId(123));
            ((ISetProvenance)buildingUnitAddressWasAttachedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>(),
                    buildingUnitAddressWasAttachedV2)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitAddressWasDetachedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            new AddressPersistentLocalId(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedV2(command.BuildingPersistentLocalId, command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithNotRealizedBuildingUnit_ThenDoNothing()
        {
            var command = Fixture.Create<NotRealizeBuildingUnit>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>(),
                    Fixture.Create<BuildingUnitWasNotRealizedV2>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithCommonBuilding_ThrowsBuildingUnitHasInvalidFunctionException()
        {
            var command = Fixture.Create<NotRealizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>
                {
                    new BuildingUnit(
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        Fixture.Create<BuildingRegistry.Legacy.PersistentLocalId>(),
                        BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        isRemoved: false)
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

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
            var command = new NotRealizeBuildingUnit(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(2),
                Fixture.Create<Provenance>());

            var commonBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(1);
            var commonBuildingUnitWasAddedV2 = Fixture.Create<CommonBuildingUnitWasAddedV2>()
                .WithBuildingUnitStatus(BuildingUnitStatus.Planned)
                .WithBuildingUnitPersistentLocalId(commonBuildingUnitPersistentLocalId);
            var buildingUnitAddressWasAttachedV2 = new BuildingUnitAddressWasAttachedV2(
                Fixture.Create<BuildingPersistentLocalId>(),
                commonBuildingUnitPersistentLocalId,
                new AddressPersistentLocalId(123));
            ((ISetProvenance)buildingUnitAddressWasAttachedV2).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                        .WithFunction(BuildingUnitFunction.Unknown),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(3))
                        .WithFunction(BuildingUnitFunction.Unknown),
                    commonBuildingUnitWasAddedV2,
                    buildingUnitAddressWasAttachedV2)
                .When(command)
                .Then(
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitAddressWasDetachedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId),
                            new AddressPersistentLocalId(123))),
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId)))));
        }

        [Fact]
        public void WithRealizedCommonBuildingUnitAndTwoOtherBuildingUnits_ThenCommonBuildingUnitWasRetired()
        {
            var command = new NotRealizeBuildingUnit(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(1),
                Fixture.Create<Provenance>());

            var commonBuildingUnitWasAddedV2 = Fixture.Create<CommonBuildingUnitWasAddedV2>()
                .WithBuildingUnitStatus(BuildingUnitStatus.Realized)
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(3));

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithFunction(BuildingUnitFunction.Unknown)
                        .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(2))
                        .WithFunction(BuildingUnitFunction.Unknown),
                    commonBuildingUnitWasAddedV2)
                .When(command)
                .Then(
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId)),
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRetiredV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId)))));
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("Realized")]
        public void WithActiveCommonBuildingUnitAndThreeOtherBuildingUnits_ThenNothingForCommonBuildingUnit(string buildingUnitStatus)
        {
            var command = new NotRealizeBuildingUnit(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(1),
                Fixture.Create<Provenance>());

            var commonBuildingUnitWasAddedV2 = Fixture.Create<CommonBuildingUnitWasAddedV2>()
                .WithBuildingUnitStatus(BuildingUnitStatus.Parse(buildingUnitStatus))
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(4));

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithFunction(BuildingUnitFunction.Unknown)
                        .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(2))
                        .WithFunction(BuildingUnitFunction.Unknown),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(3))
                        .WithFunction(BuildingUnitFunction.Unknown),
                    commonBuildingUnitWasAddedV2)
                .When(command)
                .Then(
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasNotRealizedV2(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void BuildingUnitIsRemoved_ThrowsBuildingUnitIsRemovedException()
        {
            var command = Fixture.Create<NotRealizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>
                {
                    new BuildingUnit(
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        Fixture.Create<BuildingRegistry.Legacy.PersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitFunction>(),
                        BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        isRemoved: true)
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitIsRemovedException(command.BuildingUnitPersistentLocalId)));
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("Realized")]
        public void WithInvalidBuildingUnitStatus_ThrowsBuildingUnitHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<NotRealizeBuildingUnit>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingUnit>
                {
                    new BuildingUnit(
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        Fixture.Create<BuildingRegistry.Legacy.PersistentLocalId>(),
                        BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Parse(status) ?? throw new ArgumentException(),
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        isRemoved: false)
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidStatusException()));
        }

        [Fact]
        public void StateCheck()
        {
            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(1));
            ((ISetProvenance)buildingUnitWasPlanned).SetProvenance(Fixture.Create<Provenance>());
            var secondBuildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(2));
            ((ISetProvenance)secondBuildingUnitWasPlanned).SetProvenance(Fixture.Create<Provenance>());
            var commonBuildingUnitWasAdded = Fixture.Create<CommonBuildingUnitWasAddedV2>()
                .WithBuildingUnitStatus(BuildingUnitStatus.Realized)
                .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(3));
            ((ISetProvenance)commonBuildingUnitWasAdded).SetProvenance(Fixture.Create<Provenance>());
            var buildingUnitAddressWasAttachedV2 = new BuildingUnitAddressWasAttachedV2(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlanned.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(123));
            ((ISetProvenance)buildingUnitAddressWasAttachedV2).SetProvenance(Fixture.Create<Provenance>());
            var commonBuildingUnitAddressWasAttachedV2 = new BuildingUnitAddressWasAttachedV2(
                Fixture.Create<BuildingPersistentLocalId>(),
                new BuildingUnitPersistentLocalId(commonBuildingUnitWasAdded.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(123));
            ((ISetProvenance)commonBuildingUnitAddressWasAttachedV2).SetProvenance(Fixture.Create<Provenance>());

            // Act
            building.Initialize(new object[]
            {
                buildingWasPlanned,
                buildingUnitWasPlanned,
                buildingUnitAddressWasAttachedV2,
                secondBuildingUnitWasPlanned,
                commonBuildingUnitWasAdded,
                commonBuildingUnitAddressWasAttachedV2
            });

            building.NotRealizeBuildingUnit(
                new BuildingUnitPersistentLocalId(buildingUnitWasPlanned.BuildingUnitPersistentLocalId));

            // Assert
            building.BuildingUnits.Should().NotBeEmpty();
            building.BuildingUnits.Count.Should().Be(3);

            var buildingUnit = building.BuildingUnits
                .Single(x => x.BuildingUnitPersistentLocalId == buildingUnitWasPlanned.BuildingUnitPersistentLocalId);
            buildingUnit.Status.Should().Be(BuildingUnitStatus.NotRealized);
            buildingUnit.AddressPersistentLocalIds.Should().BeEmpty();

            var commonBuildingUnit = building.BuildingUnits
                .Single(x => x.BuildingUnitPersistentLocalId == commonBuildingUnitWasAdded.BuildingUnitPersistentLocalId);
            commonBuildingUnit.Status.Should().Be(BuildingUnitStatus.Retired);
            commonBuildingUnit.AddressPersistentLocalIds.Should().BeEmpty();
        }
    }
}
