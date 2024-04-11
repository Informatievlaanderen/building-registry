namespace BuildingRegistry.Tests.AggregateTests.WhenMovingBuildingUnitIntoBuilding
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Events;
    using BuildingRegistry.Tests.Extensions;
    using BuildingRegistry.Tests.Fixtures;
    using System.Collections.Generic;
    using WhenPlanningBuildingUnit;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingGeometryMethod = Building.BuildingGeometryMethod;
    using BuildingUnitPositionGeometryMethod = Building.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = Building.BuildingUnitStatus;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

    public class GivenCommonBuildingUnitExists : BuildingRegistryTest
    {
        public GivenCommonBuildingUnitExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithActiveBuildingUnit_ThenCommonBuildingUnitWasNotAdded()
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
                    ),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        Fixture.Create<CommonBuildingUnitWasAddedV2>()
                            .WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)
                            .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(1))
                            .WithBuildingUnitStatus(BuildingUnitStatus.Planned)
                            .WithGeometry(buildingGeometry.Center)
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
                            new List<AddressPersistentLocalId>()))
                ));
        }

        [Fact]
        public void WithInactiveCommonBuildingUnit_ThenCommonBuildingUnitIsReused()
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
                    ),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        Fixture.Create<CommonBuildingUnitWasAddedV2>()
                            .WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)
                            .WithBuildingUnitPersistentLocalId(new BuildingUnitPersistentLocalId(1))
                            .WithBuildingUnitStatus(BuildingUnitStatus.NotRealized)
                            .WithGeometry(buildingGeometry.Center)
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
                        new BuildingUnitWasCorrectedFromNotRealizedToPlanned(
                            command.DestinationBuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1)))
                ));
        }
    }
}
