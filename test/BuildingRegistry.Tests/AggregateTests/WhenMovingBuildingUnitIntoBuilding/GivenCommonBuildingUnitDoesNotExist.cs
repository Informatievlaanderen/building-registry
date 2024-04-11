namespace BuildingRegistry.Tests.AggregateTests.WhenMovingBuildingUnitIntoBuilding
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Events;
    using BuildingRegistry.Tests.Extensions;
    using System.Collections.Generic;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenCommonBuildingUnitDoesNotExist : BuildingRegistryTest
    {
        public GivenCommonBuildingUnitDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public void WithSingleBuildingUnit_ThenCommonBuildingUnitWasAdded()
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();

            var sourceBuildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>()
                .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId);
            var sourceBuildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                .WithGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithDeviation(false);

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(sourceBuildingWasPlanned.ExtendedWkbGeometry), BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId), sourceBuildingWasPlanned),
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId), sourceBuildingUnitWasPlanned),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)
                    ),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        Fixture.Create<BuildingUnitWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)
                    )
                )
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        new BuildingUnitWasMovedIntoBuilding(
                            new BuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId),
                            new BuildingPersistentLocalId(command.SourceBuildingPersistentLocalId),
                            new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                            BuildingUnitStatus.Planned,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            BuildingUnitFunction.Unknown,
                            false,
                            new List<AddressPersistentLocalId>())),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        new CommonBuildingUnitWasAddedV2(
                            command.DestinationBuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingUnitStatus.Planned,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            false))));
        }

        [Fact]
        public void WithoutActiveBuildingUnit_ThenCommonBuildingUnitWasNotAdded()
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();

            var removedBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithPersistentLocalId(123)
                .WithStatus(BuildingRegistry.Legacy.BuildingUnitStatus.Planned)
                .WithFunction(BuildingRegistry.Legacy.BuildingUnitFunction.Unknown)
                .WithIsRemoved()
                .Build();

            var retiredBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithPersistentLocalId(456)
                .WithStatus(BuildingRegistry.Legacy.BuildingUnitStatus.Retired)
                .WithFunction(BuildingRegistry.Legacy.BuildingUnitFunction.Unknown)
                .Build();

            var notRealizedBuildingUnit = new BuildingUnitBuilder(Fixture)
                .WithPersistentLocalId(789)
                .WithStatus(BuildingRegistry.Legacy.BuildingUnitStatus.NotRealized)
                .WithFunction(BuildingRegistry.Legacy.BuildingUnitFunction.Unknown)
                .Build();

            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()), BuildingGeometryMethod.Outlined);

            var destinationBuildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingUnit(removedBuildingUnit)
                .WithBuildingUnit(retiredBuildingUnit)
                .WithBuildingUnit(notRealizedBuildingUnit)
                .Build();

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingUnitWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                            .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                            .WithDeviation(false)),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId), destinationBuildingWasMigrated)
                )
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        new BuildingUnitWasMovedIntoBuilding(
                            new BuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId),
                            new BuildingPersistentLocalId(command.SourceBuildingPersistentLocalId),
                            new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                            BuildingUnitStatus.Planned,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            buildingGeometry.Center,
                            BuildingUnitFunction.Unknown,
                            false,
                            new List<AddressPersistentLocalId>()
                        ))));
        }
    }
}
