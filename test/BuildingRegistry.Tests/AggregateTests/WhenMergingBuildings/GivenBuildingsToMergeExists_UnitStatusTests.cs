// namespace BuildingRegistry.Tests.AggregateTests.WhenMergingBuildings
// {
//     using System.Collections.Generic;
//     using System.Linq;
//     using AutoFixture;
//     using Be.Vlaanderen.Basisregisters.AggregateSource;
//     using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
//     using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
//     using Building;
//     using Building.Commands;
//     using Building.Events;
//     using Xunit;
//
//     public partial class GivenBuildingsToMergeExists
//     {
//         [Fact]
//         public void WithBuildingUnitsInInvalidStatus_ThenBuildingMergerWasRealizedWithoutUnitsTransferred()
//         {
//             var buildingWasPlannedEvents = Fixture.CreateMany<BuildingWasPlannedV2>(_random.Next(2, 20)).ToList();
//
//             var givenPlannedFacts = buildingWasPlannedEvents
//                 .Select(x =>
//                     new Fact(new BuildingStreamId(new BuildingPersistentLocalId(x.BuildingPersistentLocalId)), x));
//
//             var firstBuilding = buildingWasPlannedEvents.First();
//             var planFirstUnit = PlanUnit(firstBuilding.BuildingPersistentLocalId);
//             var notRealizeFirstUnit = new BuildingUnitWasNotRealizedV2(new BuildingPersistentLocalId(firstBuilding.BuildingPersistentLocalId),
//                 new BuildingUnitPersistentLocalId(planFirstUnit.BuildingUnitPersistentLocalId));
//             ((ISetProvenance)notRealizeFirstUnit).SetProvenance(Fixture.Create<Provenance>());
//
//             var secondBuilding = buildingWasPlannedEvents.Skip(1).First();
//             var planSecondUnit = PlanUnit(secondBuilding.BuildingPersistentLocalId);
//             var retireSecondUnit = new BuildingUnitWasRetiredV2(new BuildingPersistentLocalId(secondBuilding.BuildingPersistentLocalId),
//                 new BuildingUnitPersistentLocalId(planSecondUnit.BuildingUnitPersistentLocalId));
//             ((ISetProvenance)retireSecondUnit).SetProvenance(Fixture.Create<Provenance>());
//
//             var command = new MergeBuildings(
//                 Fixture.Create<BuildingPersistentLocalId>(),
//                 Fixture.Create<ExtendedWkbGeometry>(),
//                 buildingWasPlannedEvents.Select(x => new BuildingPersistentLocalId(x.BuildingPersistentLocalId)),
//                 Fixture.Create<Provenance>()
//             );
//
//             Assert(new Scenario()
//                 .Given(givenPlannedFacts
//                     .Concat(RealizePlannedBuildingsEvents(buildingWasPlannedEvents))
//                     .Concat(MeasurePlannedBuildingsEvents(buildingWasPlannedEvents))
//                     .Concat(new List<Fact>
//                     {
//                         CreateFact(firstBuilding.BuildingPersistentLocalId, planFirstUnit),
//                         CreateFact(firstBuilding.BuildingPersistentLocalId, notRealizeFirstUnit),
//                         CreateFact(secondBuilding.BuildingPersistentLocalId, planSecondUnit),
//                         CreateFact(secondBuilding.BuildingPersistentLocalId, retireSecondUnit)
//                     })
//                     .ToArray())
//                 .When(command)
//                 .Then(new Fact(new BuildingStreamId(command.NewBuildingPersistentLocalId),
//                     command.ToBuildingMergerWasRealizedEvent())));
//         }
//     }
// }
