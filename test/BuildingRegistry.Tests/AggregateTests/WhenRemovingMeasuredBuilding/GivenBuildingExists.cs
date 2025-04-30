namespace BuildingRegistry.Tests.AggregateTests.WhenRemovingMeasuredBuilding
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void ThenBuildingWasRemoved()
        {
            var command = Fixture.Create<RemoveMeasuredBuilding>();

            var buildingWasPlannedV2 = Fixture.Create<BuildingWasPlannedV2>();
            var buildingWasMeasured = new BuildingWasMeasured(
                Fixture.Create<BuildingPersistentLocalId>(),
                [],
                [],
                new ExtendedWkbGeometry(buildingWasPlannedV2.ExtendedWkbGeometry.ToByteArray()!),
                null);
            ((ISetProvenance)buildingWasMeasured).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>())!,
                    buildingWasPlannedV2,
                    buildingWasMeasured)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId)!,
                    new BuildingWasRemovedV2(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithBuildingUnits_ThenBuildingHasBuildingUnitsIsThrown()
        {
            var command = Fixture.Create<RemoveMeasuredBuilding>();

            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(
                    new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                    BuildingGeometryMethod.MeasuredByGrb))
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    buildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId>() { new AddressPersistentLocalId(1) },
                    isRemoved: false)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    isRemoved: true)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingHasBuildingUnitsException()));
        }

        [Fact]
        public void WithIsRemoved_ThenDoNothing()
        {
            var command = Fixture.Create<RemoveMeasuredBuilding>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithIsRemoved()
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithGeometryMethodOtherThanMeasured_ThenThrowsBuildingHasInvalidBuildingGeometryMethodException()
        {
            var command = Fixture.Create<RemoveMeasuredBuilding>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(
                    new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                    BuildingGeometryMethod.Outlined))
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
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(
                    new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                    BuildingGeometryMethod.MeasuredByGrb))
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    buildingUnitPersistentLocalId,
                    isRemoved: true)
                .WithBuildingUnit(
                    BuildingUnitStatus.Planned,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    isRemoved: true)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { buildingWasMigrated });

            // Act
            sut.RemoveMeasuredBuilding();

            // Assert
            sut.IsRemoved.Should().BeTrue();

            foreach (var buildingUnit in sut.BuildingUnits)
            {
                buildingUnit.IsRemoved.Should().BeTrue();
            }
        }
    }
}
