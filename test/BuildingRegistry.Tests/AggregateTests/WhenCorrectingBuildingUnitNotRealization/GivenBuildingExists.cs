namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitNotRealization
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
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingId = Building.BuildingId;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnit = Building.Commands.BuildingUnit;
    using BuildingUnitStatus = Building.BuildingUnitStatus;

    public partial class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void WhenNotRealizedBuildingUnit_ThenBuildingUnitWasCorrectedToPlanned()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingExtendedWkbGeometry = new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary());
            var buildingGeometry = new BuildingGeometry(
                buildingExtendedWkbGeometry,
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>()
                        .WithGeometry(buildingExtendedWkbGeometry),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithFunction(BuildingUnitFunction.Unknown)
                        .WithPosition(
                            new BuildingUnitPosition(buildingGeometry.Center,
                                BuildingUnitPositionGeometryMethod.AppointedByAdministrator)),
                    Fixture.Create<BuildingUnitWasNotRealizedV2>())
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithPositionDerivedFromObject_ThenBuildingUnitWasCorrectedToPlannedAndPositionCorrected()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingGeometry = new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary());
            var expectedPosition = new BuildingGeometry(buildingGeometry, BuildingGeometryMethod.Outlined).Center;

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>()
                        .WithGeometry(buildingGeometry),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithFunction(BuildingUnitFunction.Unknown)
                        .WithPosition(
                            new BuildingUnitPosition(
                                new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                                BuildingUnitPositionGeometryMethod.DerivedFromObject)),
                    Fixture.Create<BuildingUnitWasNotRealizedV2>())
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitPositionWasCorrected(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            expectedPosition)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WithPositionAppointedByAdminAndOutsideOfBuildingGeometry_ThenBuildingUnitWasCorrectedToPlannedAndPositionCorrected()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>()
                        .WithGeometry(new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary())),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>()
                        .WithFunction(BuildingUnitFunction.Unknown)
                        .WithPosition(
                            new BuildingUnitPosition(
                                new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                                BuildingUnitPositionGeometryMethod.AppointedByAdministrator)),
                    Fixture.Create<BuildingUnitWasNotRealizedV2>())
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitPositionWasCorrected(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId))));
        }

        [Fact]
        public void WhenPlannedBuildingUnit_ThenDoNothing()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingWasRealizedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>())
                .When(command)
                .ThenNone());
        }

        [Theory]
        [InlineData("Realized")]
        [InlineData("Retired")]
        public void WithInvalidBuildingUnitStatus_ThrowsBuildingUnitHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

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

        [Theory]
        [InlineData("NotRealized")]
        [InlineData("Retired")]
        public void WithInvalidBuildingStatus_ThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Parse(status),
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false, new List<BuildingUnit>
                {
                    new BuildingUnit(
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        Fixture.Create<BuildingRegistry.Legacy.PersistentLocalId>(),
                        BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.NotRealized,
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
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Fact]
        public void WithCommonBuilding_ThrowsBuildingUnitHasInvalidFunctionException()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

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
                        BuildingRegistry.Legacy.BuildingUnitStatus.NotRealized,
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
        public void NonExistentBuildingUnit_ThrowsBuildingUnitNotFoundException()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
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
                .Throws(new BuildingUnitIsNotFoundException()));
        }

        [Fact]
        public void BuildingUnitIsRemoved_ThrowsBuildingUnitIsRemovedException()
        {
            var command = Fixture.Create<CorrectBuildingUnitNotRealization>();

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

        [Fact]
        public void StateCheck()
        {
            // Arrange
            var building = new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>()).Create();

            // Act
            building.Initialize(new object[]
            {
                Fixture.Create<BuildingWasPlannedV2>(),
                Fixture.Create<BuildingUnitWasPlannedV2>(),
                Fixture.Create<BuildingUnitWasNotRealizedV2>(),
                Fixture.Create<BuildingUnitWasCorrectedFromRealizedToPlanned>()
            });

            // Assert
            building.BuildingUnits.Should().NotBeEmpty();
            building.BuildingUnits.Count.Should().Be(1);

            var buildingUnit = building.BuildingUnits.First();
            buildingUnit.Status.Should().Be(BuildingUnitStatus.Planned);
            buildingUnit.LastEventHash.Should().NotBe(building.LastEventHash);
        }
    }
}
