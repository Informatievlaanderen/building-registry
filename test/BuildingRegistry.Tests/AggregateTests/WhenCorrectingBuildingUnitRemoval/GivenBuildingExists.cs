#pragma warning disable CS0618 // Type or member is obsolete
namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitRemoval
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
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingGeometryMethod = Building.BuildingGeometryMethod;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitPositionGeometryMethod = BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public void WithRemovedBuildingUnit_ThenBuildingUnitRemovalWasCorrected()
        {
            var command = Fixture.Create<CorrectBuildingUnitRemoval>();

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    buildingUnitPersistentLocalId: command.BuildingUnitPersistentLocalId,
                    status: BuildingUnitStatus.Planned,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    isRemoved: true)
                .Build();
            var buildingUnit = buildingWasMigrated.BuildingUnits.Single();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitRemovalWasCorrected(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        BuildingRegistry.Building.BuildingUnitStatus.Parse(buildingUnit.Status),
                        BuildingRegistry.Building.BuildingUnitFunction.Parse(buildingUnit.Function),
                        BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod),
                        buildingGeometry.Center,
                        false))));
        }

        [Fact]
        public void WithPositionOutsideOfBuilding_ThenBuildingUnitPositionWasCorrected()
        {
            var command = Fixture.Create<CorrectBuildingUnitRemoval>();

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    buildingUnitPersistentLocalId: command.BuildingUnitPersistentLocalId,
                    status: BuildingUnitStatus.Planned,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                    isRemoved: true)
                .Build();
            var buildingUnit = buildingWasMigrated.BuildingUnits.Single();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitRemovalWasCorrected(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            BuildingRegistry.Building.BuildingUnitStatus.Parse(buildingUnit.Status),
                            BuildingRegistry.Building.BuildingUnitFunction.Parse(buildingUnit.Function),
                            BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false))
                ));
        }

        [Fact]
        public void WithNotRemovedBuildingUnit_ThenDoNothing()
        {
            var command = Fixture.Create<CorrectBuildingUnitRemoval>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    Fixture.Create<BuildingUnitWasPlannedV2>())
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithBuildingIsRemoved_ThenThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<CorrectBuildingUnitRemoval>();

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

        [Theory]
        [InlineData("NotRealized")]
        [InlineData("Retired")]
        public void WithInvalidBuildingStatus_ThenThrowsBuildingHasInvalidStatusException(string buildingStatus)
        {
            var command = Fixture.Create<CorrectBuildingUnitRemoval>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Parse(buildingStatus))
                .WithBuildingUnit(
                    buildingUnitPersistentLocalId: command.BuildingUnitPersistentLocalId,
                    status: BuildingUnitStatus.Planned)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("UnderConstruction")]
        public void WithBuildingUnitStatusBehindBuildingUnitStatus_ThenCorrectBuildingUnitStatusException(string buildingStatus)
        {
            var command = Fixture.Create<CorrectBuildingUnitRemoval>();

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Parse(buildingStatus))
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    buildingUnitPersistentLocalId: command.BuildingUnitPersistentLocalId,
                    status: BuildingUnitStatus.Realized,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject,
                    isRemoved: true)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(
                        new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitRemovalWasCorrected(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            BuildingRegistry.Building.BuildingUnitStatus.Planned,
                            BuildingRegistry.Building.BuildingUnitFunction.Unknown,
                            BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false))));
        }

        [Fact]
        public void WithCommonBuilding_ThenThrowsBuildingUnitHasInvalidFunctionException()
        {
            var command = Fixture.Create<CorrectBuildingUnitRemoval>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                    .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                    .WithBuildingStatus(BuildingStatus.Realized)
                    .WithBuildingUnit(
                        BuildingUnitStatus.Realized,
                        command.BuildingUnitPersistentLocalId,
                        BuildingUnitFunction.Common)
                    .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidFunctionException()));
        }

        [Fact]
        public void NonExistentBuildingUnit_ThenThrowsBuildingUnitNotFoundException()
        {
            var command = Fixture.Create<CorrectBuildingUnitRemoval>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Realized)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitIsNotFoundException()));
        }

        [Fact]
        public void StateCheck()
        {
            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.Outlined);

            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    buildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: new BuildingRegistry.Legacy.ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary()),
                    attachedAddresses: Fixture.CreateMany<AddressPersistentLocalId>().ToList())
                .Build();

            var buildingUnitRemovalWasCorrected = new BuildingUnitRemovalWasCorrected(
                Fixture.Create<BuildingPersistentLocalId>(),
                buildingUnitPersistentLocalId,
                BuildingRegistry.Building.BuildingUnitStatus.Planned,
                BuildingRegistry.Building.BuildingUnitFunction.Unknown,
                BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject,
                buildingGeometry.Center,
                false);
            ((ISetProvenance)buildingUnitRemovalWasCorrected).SetProvenance(Fixture.Create<Provenance>());

            // Act
            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize(new object[]
            {
                buildingWasMigrated, buildingUnitRemovalWasCorrected
            });

            // Assert
            var buildingUnit = building.BuildingUnits.Single();

            buildingUnit.Function.Should().Be(BuildingRegistry.Building.BuildingUnitFunction.Parse(buildingUnitRemovalWasCorrected.Function));
            buildingUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Parse(buildingUnitRemovalWasCorrected.BuildingUnitStatus));
            buildingUnit.BuildingUnitPosition.Geometry.Should().Be(new ExtendedWkbGeometry(buildingUnitRemovalWasCorrected.ExtendedWkbGeometry));
            buildingUnit.BuildingUnitPosition.GeometryMethod.Should().Be(BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.Parse(buildingUnitRemovalWasCorrected.GeometryMethod));
            buildingUnit.HasDeviation.Should().Be(buildingUnitRemovalWasCorrected.HasDeviation);
            buildingUnit.IsRemoved.Should().BeFalse();
            buildingUnit.AddressPersistentLocalIds.Should().BeEmpty();

            buildingUnit.LastEventHash.Should().Be(buildingUnitRemovalWasCorrected.GetHash());
            buildingUnit.LastEventHash.Should().Be(building.LastEventHash);
        }
    }
}
