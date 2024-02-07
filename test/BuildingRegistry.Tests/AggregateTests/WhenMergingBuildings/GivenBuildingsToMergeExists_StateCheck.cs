// namespace BuildingRegistry.Tests.AggregateTests.WhenMergingBuildings
// {
//     using System.Collections.Generic;
//     using System.Linq;
//     using Autofac;
//     using AutoFixture;
//     using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
//     using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
//     using Building;
//     using Building.Commands;
//     using Building.Datastructures;
//     using Building.Events;
//     using Extensions;
//     using FluentAssertions;
//     using Moq;
//     using Xunit;
//     using BuildingUnit = Building.BuildingUnit;
//
//     public partial class GivenBuildingsToMergeExists
//     {
//         [Fact]
//         public void StateCheck()
//         {
//             var buildingPersistentLocalId = new BuildingPersistentLocalId(123);
//
//             var buildingMergerWasRealized = new BuildingMergerWasRealized(
//                 buildingPersistentLocalId,
//                 new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
//                 new List<BuildingPersistentLocalId>(Fixture.CreateMany<BuildingPersistentLocalId>(3))
//             );
//             buildingMergerWasRealized.SetFixtureProvenance(Fixture);
//
//             var unitTransferred1 = new BuildingUnitWasTransferred(
//                 buildingPersistentLocalId,
//                 BuildingUnit.Transfer(_ => { },
//                     buildingPersistentLocalId,
//                     new BuildingUnitPersistentLocalId(111),
//                     BuildingUnitFunction.Unknown,
//                     BuildingUnitStatus.Realized,
//                     Fixture.Create<List<AddressPersistentLocalId>>(),
//                     new BuildingUnitPosition(NewBuildingCenter, BuildingUnitPositionGeometryMethod.DerivedFromObject),
//                     hasDeviation: Fixture.Create<bool>()),
//                 Fixture.Create<BuildingPersistentLocalId>(),
//                 new BuildingUnitPosition(NewBuildingCenter, BuildingUnitPositionGeometryMethod.DerivedFromObject)
//             );
//             unitTransferred1.SetFixtureProvenance(Fixture);
//
//
//             var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
//             building.Initialize(new object[]
//             {
//                 buildingMergerWasRealized,
//                 unitTransferred1
//             });
//
//             building.BuildingPersistentLocalId.Should().Be(new BuildingPersistentLocalId(buildingMergerWasRealized.BuildingPersistentLocalId));
//             building.BuildingStatus.Should().Be(BuildingStatus.Realized);
//             building.BuildingGeometry.Geometry.Should().Be(new ExtendedWkbGeometry(buildingMergerWasRealized.ExtendedWkbGeometry));
//             building.BuildingGeometry.Method.Should().Be(BuildingGeometryMethod.MeasuredByGrb);
//             building.LastEventHash.Should().Be(unitTransferred1.GetHash());
//
//             building.BuildingUnits[0].BuildingUnitPersistentLocalId
//                 .Should()
//                 .Be(new BuildingUnitPersistentLocalId(unitTransferred1.BuildingUnitPersistentLocalId));
//             building.BuildingUnits[0].Status.Should().Be(BuildingUnitStatus.Realized);
//             building.BuildingUnits[0].HasDeviation.Should().Be(unitTransferred1.HasDeviation);
//             building.BuildingUnits[0].IsRemoved.Should().BeFalse();
//             building.BuildingUnits[0].BuildingUnitPosition.Geometry.Should().Be(NewBuildingCenter);
//             building.BuildingUnits[0].BuildingUnitPosition.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject);
//             building.BuildingUnits[0].AddressPersistentLocalIds
//                 .Should()
//                 .BeEquivalentTo(unitTransferred1.AddressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)));
//             building.BuildingUnits[0].LastEventHash.Should().Be(unitTransferred1.GetHash());
//         }
//     }
// }
