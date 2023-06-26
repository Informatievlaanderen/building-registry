namespace BuildingRegistry.Tests.AggregateTests.WhenMergingBuildings
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Xunit;
    using BuildingUnit = Building.BuildingUnit;

    public partial class GivenBuildingsToMergeExists
    {
        [Fact]
        public void WithBuildingUnitOutsideNewBuilding_ThenBuildingUnitWasTransferredWithCenteredPosition()
        {
            var buildingWasPlannedEvents = Fixture.CreateMany<BuildingWasPlannedV2>(2).ToList();

            var givenPlannedFacts = buildingWasPlannedEvents
                .Select(x =>
                    new Fact(new BuildingStreamId(new BuildingPersistentLocalId(x.BuildingPersistentLocalId)), x));

            var command = new MergeBuildings(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                buildingWasPlannedEvents.Select(x => new BuildingPersistentLocalId(x.BuildingPersistentLocalId)),
                Fixture.Create<Provenance>()
            );

            var buildingUnitWasPlannedV2 = new BuildingUnitWasPlannedV2(
                new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                Fixture.Create<BuildingUnitFunction>(),
                Fixture.Create<bool>());
            ((ISetProvenance) buildingUnitWasPlannedV2).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitWasRealizedV2 = new BuildingUnitWasRealizedV2(
                new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId));
            ((ISetProvenance) buildingUnitWasRealizedV2).SetProvenance(Fixture.Create<Provenance>());

            // Assert
            var expectedBuildingUnitPosition = new BuildingUnitPosition(
                new BuildingGeometry(command.NewExtendedWkbGeometry, BuildingGeometryMethod.MeasuredByGrb).Center,
                BuildingUnitPositionGeometryMethod.DerivedFromObject);

            Assert(new Scenario()
                .Given(givenPlannedFacts
                    .Concat(RealizePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(MeasurePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(new List<Fact>
                    {
                        new Fact(
                            new BuildingStreamId(
                                new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId)),
                            buildingUnitWasPlannedV2),
                        new Fact(
                            new BuildingStreamId(
                                new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId)),
                            buildingUnitWasRealizedV2)
                    })
                    .ToArray())
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.NewBuildingPersistentLocalId),
                        command.ToBuildingMergerWasRealizedEvent()),
                    new Fact(new BuildingStreamId(command.NewBuildingPersistentLocalId), new BuildingUnitWasTransferred(
                        new BuildingPersistentLocalId(command.NewBuildingPersistentLocalId),
                        BuildingUnit.Transfer(_ => { },
                            command.NewBuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                            BuildingUnitFunction.Unknown,
                            BuildingUnitStatus.Realized,
                            new List<AddressPersistentLocalId>(),
                            expectedBuildingUnitPosition,
                            buildingUnitWasPlannedV2.HasDeviation),
                        new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId),
                        expectedBuildingUnitPosition))));
        }

        [Fact]
        public void WithDerivedBuildingUnitInsideNewBuilding_ThenBuildingUnitWasTransferredWithCenteredPosition()
        {
            var buildingWasPlannedEvents = Fixture.CreateMany<BuildingWasPlannedV2>(2).ToList();

            var givenPlannedFacts = buildingWasPlannedEvents
                .Select(x =>
                    new Fact(new BuildingStreamId(new BuildingPersistentLocalId(x.BuildingPersistentLocalId)), x));

            var command = new MergeBuildings(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                buildingWasPlannedEvents.Select(x => new BuildingPersistentLocalId(x.BuildingPersistentLocalId)),
                Fixture.Create<Provenance>()
            );

            var buildingUnitWasPlannedV2 = new BuildingUnitWasPlannedV2(
                new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                new ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary()),
                Fixture.Create<BuildingUnitFunction>(),
                Fixture.Create<bool>());
            ((ISetProvenance) buildingUnitWasPlannedV2).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitWasRealizedV2 = new BuildingUnitWasRealizedV2(
                new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId));
            ((ISetProvenance) buildingUnitWasRealizedV2).SetProvenance(Fixture.Create<Provenance>());

            // Assert
            var expectedBuildingUnitPosition = new BuildingUnitPosition(
                new BuildingGeometry(command.NewExtendedWkbGeometry, BuildingGeometryMethod.MeasuredByGrb).Center,
                BuildingUnitPositionGeometryMethod.DerivedFromObject);

            Assert(new Scenario()
                .Given(givenPlannedFacts
                    .Concat(RealizePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(MeasurePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(new List<Fact>
                    {
                        new Fact(
                            new BuildingStreamId(
                                new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId)),
                            buildingUnitWasPlannedV2),
                        new Fact(
                            new BuildingStreamId(
                                new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId)),
                            buildingUnitWasRealizedV2)
                    })
                    .ToArray())
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.NewBuildingPersistentLocalId),
                        command.ToBuildingMergerWasRealizedEvent()),
                    new Fact(new BuildingStreamId(command.NewBuildingPersistentLocalId), new BuildingUnitWasTransferred(
                        new BuildingPersistentLocalId(command.NewBuildingPersistentLocalId),
                        BuildingUnit.Transfer(_ => { },
                            command.NewBuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                            BuildingUnitFunction.Unknown,
                            BuildingUnitStatus.Realized,
                            new List<AddressPersistentLocalId>(),
                            expectedBuildingUnitPosition,
                            buildingUnitWasPlannedV2.HasDeviation),
                        new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId),
                        expectedBuildingUnitPosition))));
        }

        [Fact]
        public void WithAppointedBuildingUnitInsideNewBuilding_ThenBuildingUnitWasTransferredWithUnchangedPosition()
        {
            var buildingWasPlannedEvents = Fixture.CreateMany<BuildingWasPlannedV2>(2).ToList();

            var givenPlannedFacts = buildingWasPlannedEvents
                .Select(x =>
                    new Fact(new BuildingStreamId(new BuildingPersistentLocalId(x.BuildingPersistentLocalId)), x));

            var command = new MergeBuildings(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                buildingWasPlannedEvents.Select(x => new BuildingPersistentLocalId(x.BuildingPersistentLocalId)),
                Fixture.Create<Provenance>()
            );

            var originalBuildingUnitPosition = new ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary());

            var buildingUnitWasPlannedV2 = new BuildingUnitWasPlannedV2(
                new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                originalBuildingUnitPosition,
                Fixture.Create<BuildingUnitFunction>(),
                Fixture.Create<bool>());
            ((ISetProvenance) buildingUnitWasPlannedV2).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitWasRealizedV2 = new BuildingUnitWasRealizedV2(
                new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId));
            ((ISetProvenance) buildingUnitWasRealizedV2).SetProvenance(Fixture.Create<Provenance>());

            // Assert
            var expectedBuildingUnitPosition = new BuildingUnitPosition(
                originalBuildingUnitPosition,
                BuildingUnitPositionGeometryMethod.AppointedByAdministrator);

            Assert(new Scenario()
                .Given(givenPlannedFacts
                    .Concat(RealizePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(MeasurePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(new List<Fact>
                    {
                        new Fact(
                            new BuildingStreamId(
                                new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId)),
                            buildingUnitWasPlannedV2),
                        new Fact(
                            new BuildingStreamId(
                                new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId)),
                            buildingUnitWasRealizedV2)
                    })
                    .ToArray())
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.NewBuildingPersistentLocalId),
                        command.ToBuildingMergerWasRealizedEvent()),
                    new Fact(new BuildingStreamId(command.NewBuildingPersistentLocalId), new BuildingUnitWasTransferred(
                        new BuildingPersistentLocalId(command.NewBuildingPersistentLocalId),
                        BuildingUnit.Transfer(_ => { },
                            command.NewBuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId),
                            BuildingUnitFunction.Unknown,
                            BuildingUnitStatus.Realized,
                            new List<AddressPersistentLocalId>(),
                            expectedBuildingUnitPosition,
                            buildingUnitWasPlannedV2.HasDeviation),
                        new BuildingPersistentLocalId(buildingWasPlannedEvents.First().BuildingPersistentLocalId),
                        expectedBuildingUnitPosition))));
        }
    }
}
