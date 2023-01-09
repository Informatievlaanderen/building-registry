namespace BuildingRegistry.Tests.AggregateTests.WhenRemovingBuilding
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
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

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void ThenBuildingWasRemoved()
        {
            var command = Fixture.Create<RemoveBuilding>();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>())
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingWasRemovedV2(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithBuildingUnits_ThenBuildingUnitsAreRemoved()
        {
            var command = Fixture.Create<RemoveBuilding>();

            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(
                    new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                    BuildingGeometryMethod.Outlined))
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                    buildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId>() { new AddressPersistentLocalId(1) },
                    isRemoved: false)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    isRemoved: true)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitAddressWasDetachedV2(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                            new AddressPersistentLocalId(1))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasRemovedBecauseBuildingWasRemoved(
                            command.BuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId))),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingWasRemovedV2(command.BuildingPersistentLocalId))));
        }

        [Fact]
        public void WithIsRemoved_ThenDoNothing()
        {
            var command = Fixture.Create<RemoveBuilding>();

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
        public void WithGeometryMethodOtherThanOutlined_ThenThrowsBuildingHasInvalidBuildingGeometryMethodException()
        {
            var command = Fixture.Create<RemoveBuilding>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(
                    new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                    BuildingGeometryMethod.MeasuredByGrb))
                .Build();

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
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(
                    new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                    BuildingGeometryMethod.Outlined))
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                    buildingUnitPersistentLocalId,
                    attachedAddresses: new List<AddressPersistentLocalId>() { new AddressPersistentLocalId(1) },
                    isRemoved: false)
                .WithBuildingUnit(
                    BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    isRemoved: true)
                .Build();

            var sut = new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>(), Mock.Of<IAddresses>()).Create();
            sut.Initialize(new List<object> { buildingWasMigrated });

            // Act
            sut.RemoveConstruction();

            // Assert
            sut.IsRemoved.Should().BeTrue();
            foreach (var buildingUnit in sut.BuildingUnits)
            {
                buildingUnit.IsRemoved.Should().BeTrue();
                buildingUnit.AddressPersistentLocalIds.Should().BeEmpty();
            }
        }
    }
}
