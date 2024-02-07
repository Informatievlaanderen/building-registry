// namespace BuildingRegistry.Tests.AggregateTests.WhenMarkingBuildingAsMerged
// {
//     using AutoFixture;
//     using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
//     using Building;
//     using Building.Commands;
//     using Building.Events;
//     using Extensions;
//     using Xunit;
//     using Xunit.Abstractions;
//     using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;
//
//     public partial class GivenBuildingDoesExist : BuildingRegistryTest
//     {
//         public GivenBuildingDoesExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
//         {
//         }
//
//         [Fact]
//         public void ThenBuildingWasMerged()
//         {
//             var command = Fixture.Create<MarkBuildingAsMerged>();
//
//             var plannedBuilding = Fixture.Create<BuildingWasPlannedV2>()
//                 .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId);
//
//             Assert(new Scenario()
//                 .Given(new BuildingStreamId(command.BuildingPersistentLocalId),
//                     plannedBuilding)
//                 .When(command)
//                 .Then(new BuildingStreamId(command.BuildingPersistentLocalId),
//                     new BuildingWasMerged(command.BuildingPersistentLocalId,
//                         command.DestinationBuildingPersistentLocalId)));
//         }
//
//         [Fact]
//         public void WithOneBuildingUnit_ThenBuildingUnitWasMoved()
//         {
//             var command = Fixture.Create<MarkBuildingAsMerged>();
//
//             var plannedBuilding = Fixture.Create<BuildingWasPlannedV2>()
//                 .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId);
//
//             var plannedBuildingUnit = Fixture.Create<BuildingUnitWasPlannedV2>()
//                 .WithFunction(BuildingUnitFunction.Unknown);
//
//             Assert(new Scenario()
//                 .Given(new BuildingStreamId(command.BuildingPersistentLocalId),
//                     plannedBuilding,
//                     plannedBuildingUnit)
//                 .When(command)
//                 .Then(new BuildingStreamId(command.BuildingPersistentLocalId),
//                     new BuildingUnitWasMoved(command.BuildingPersistentLocalId,
//                         new BuildingUnitPersistentLocalId(plannedBuildingUnit.BuildingUnitPersistentLocalId),
//                         command.DestinationBuildingPersistentLocalId),
//                     new BuildingWasMerged(command.BuildingPersistentLocalId,
//                         command.DestinationBuildingPersistentLocalId)
//                 ));
//         }
//
//         [Fact]
//         public void WithThreeBuildingUnits_ThenBuildingUnitWasMoved()
//         {
//             var command = Fixture.Create<MarkBuildingAsMerged>();
//
//             var plannedBuilding = Fixture.Create<BuildingWasPlannedV2>()
//                 .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId);
//
//             var plannedBuildingUnit1 = Fixture.Create<BuildingUnitWasPlannedV2>()
//                 .WithBuildingUnitPersistentLocalId(1)
//                 .WithFunction(BuildingUnitFunction.Unknown);
//
//             var plannedBuildingUnit2 = Fixture.Create<BuildingUnitWasPlannedV2>()
//                 .WithBuildingUnitPersistentLocalId(2)
//                 .WithFunction(BuildingUnitFunction.Unknown);
//
//             var plannedBuildingUnit3 = Fixture.Create<BuildingUnitWasPlannedV2>()
//                 .WithBuildingUnitPersistentLocalId(3)
//                 .WithFunction(BuildingUnitFunction.Unknown);
//
//             var commonBuildingUnit = Fixture.Create<BuildingUnitWasPlannedV2>()
//                 .WithBuildingUnitPersistentLocalId(4)
//                 .WithFunction(BuildingUnitFunction.Common);
//
//             var buildingUnitAddressWasAttached = new BuildingUnitAddressWasAttachedBuilder(Fixture)
//                 .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
//                 .WithBuildingUnitPersistentLocalId(commonBuildingUnit.BuildingUnitPersistentLocalId)
//                 .WithAddressPersistentLocalId(1)
//                 .Build();
//
//             Assert(new Scenario()
//                 .Given(new BuildingStreamId(command.BuildingPersistentLocalId),
//                     plannedBuilding,
//                     plannedBuildingUnit1,
//                     plannedBuildingUnit2,
//                     commonBuildingUnit,
//                     buildingUnitAddressWasAttached,
//                     plannedBuildingUnit3)
//                 .When(command)
//                 .Then(new BuildingStreamId(command.BuildingPersistentLocalId),
//                     new BuildingUnitWasMoved(command.BuildingPersistentLocalId,
//                         new BuildingUnitPersistentLocalId(plannedBuildingUnit1.BuildingUnitPersistentLocalId),
//                         command.DestinationBuildingPersistentLocalId),
//                     new BuildingUnitWasMoved(command.BuildingPersistentLocalId,
//                         new BuildingUnitPersistentLocalId(plannedBuildingUnit2.BuildingUnitPersistentLocalId),
//                         command.DestinationBuildingPersistentLocalId),
//                     new BuildingUnitAddressWasDetachedV2(command.BuildingPersistentLocalId,
//                         new BuildingUnitPersistentLocalId(commonBuildingUnit.BuildingUnitPersistentLocalId),
//                         new AddressPersistentLocalId(1)),
//                     new BuildingUnitWasNotRealizedV2(command.BuildingPersistentLocalId,
//                         new BuildingUnitPersistentLocalId(commonBuildingUnit.BuildingUnitPersistentLocalId)),
//                     new BuildingUnitWasMoved(command.BuildingPersistentLocalId,
//                         new BuildingUnitPersistentLocalId(plannedBuildingUnit3.BuildingUnitPersistentLocalId),
//                         command.DestinationBuildingPersistentLocalId),
//                     new BuildingWasMerged(command.BuildingPersistentLocalId,
//                         command.DestinationBuildingPersistentLocalId)
//                 ));
//         }
//
//         [Theory]
//         [InlineData("Retired")]
//         [InlineData("NotRealized")]
//         public void WithInvalidBuildingUnitStatus_ThenBuildingUnitWasNotMoved(string buildingUnitStatus)
//         {
//             var command = Fixture.Create<MarkBuildingAsMerged>();
//
//             var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
//                 .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
//                 .WithBuildingUnit(BuildingUnitStatus.Parse(buildingUnitStatus)!.Value)
//                 .Build();
//
//             Assert(new Scenario()
//                 .Given(new BuildingStreamId(command.BuildingPersistentLocalId),
//                     buildingWasMigrated)
//                 .When(command)
//                 .Then(new BuildingStreamId(command.BuildingPersistentLocalId),
//                     new BuildingWasMerged(command.BuildingPersistentLocalId,
//                         command.DestinationBuildingPersistentLocalId)
//                 ));
//         }
//
//         [Fact]
//         public void WithRemovedBuildingUnitStatus_ThenBuildingUnitWasNotMoved()
//         {
//             var command = Fixture.Create<MarkBuildingAsMerged>();
//
//             var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
//                 .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
//                 .WithBuildingUnit(BuildingUnitStatus.Planned, isRemoved: true)
//                 .Build();
//
//             Assert(new Scenario()
//                 .Given(new BuildingStreamId(command.BuildingPersistentLocalId),
//                     buildingWasMigrated)
//                 .When(command)
//                 .Then(new BuildingStreamId(command.BuildingPersistentLocalId),
//                     new BuildingWasMerged(command.BuildingPersistentLocalId,
//                         command.DestinationBuildingPersistentLocalId)
//                 ));
//         }
//     }
// }
