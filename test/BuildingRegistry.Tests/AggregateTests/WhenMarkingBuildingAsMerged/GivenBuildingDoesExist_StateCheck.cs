// namespace BuildingRegistry.Tests.AggregateTests.WhenMarkingBuildingAsMerged
// {
//     using System.Linq;
//     using AutoFixture;
//     using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
//     using Building;
//     using Building.Events;
//     using Extensions;
//     using FluentAssertions;
//     using Xunit;
//
//     public partial class GivenBuildingDoesExist
//     {
//         [Fact]
//         public void StateCheck()
//         {
//             var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>()
//                 .WithBuildingPersistentLocalId(1);
//
//             var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
//                 .WithBuildingUnitPersistentLocalId(1);
//
//             var commonBuildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
//                 .WithBuildingUnitPersistentLocalId(2)
//                 .WithFunction(BuildingUnitFunction.Common);
//
//             var commonBuildingUnitWasNotRealized = Fixture.Create<BuildingUnitWasNotRealizedV2>()
//                 .WithBuildingUnitPersistentLocalId(2);
//
//             var buildingWasMerged = Fixture.Create<BuildingWasMerged>()
//                 .WithSourceBuildingPersistentLocalId(buildingUnitWasPlanned.BuildingPersistentLocalId)
//                 .WithDestinationBuildingPersistentLocalId(2);
//
//             var buildingUnitWasMoved = new BuildingUnitWasMoved(
//                 new BuildingPersistentLocalId(buildingWasPlanned.BuildingPersistentLocalId),
//                 new BuildingUnitPersistentLocalId(buildingUnitWasPlanned.BuildingUnitPersistentLocalId),
//                 new BuildingPersistentLocalId(buildingWasMerged.DestinationBuildingPersistentLocalId));
//             buildingUnitWasMoved.SetFixtureProvenance(Fixture);
//
//             var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
//             building.Initialize(new object[]
//             {
//                 buildingWasPlanned,
//                 buildingUnitWasPlanned,
//                 commonBuildingUnitWasPlanned,
//                 commonBuildingUnitWasNotRealized,
//                 buildingUnitWasMoved,
//                 buildingWasMerged
//             });
//
//             building.BuildingStatus.Should().Be(BuildingStatus.Retired);
//             building.BuildingUnits
//                 .FirstOrDefault(x =>
//                     x.BuildingUnitPersistentLocalId ==
//                     new BuildingUnitPersistentLocalId(buildingUnitWasPlanned.BuildingUnitPersistentLocalId))
//                 .Should()
//                 .BeNull();
//             var commonBuildingUnit = building.BuildingUnits
//                 .FirstOrDefault(x =>
//                     x.BuildingUnitPersistentLocalId ==
//                     new BuildingUnitPersistentLocalId(commonBuildingUnitWasPlanned.BuildingUnitPersistentLocalId));
//             commonBuildingUnit.Should().NotBeNull();
//             commonBuildingUnit!.Status.Should().Be(BuildingUnitStatus.NotRealized);
//         }
//     }
// }
