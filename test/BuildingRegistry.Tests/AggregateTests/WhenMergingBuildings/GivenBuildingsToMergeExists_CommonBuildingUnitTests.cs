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
        public void WithTwoBuildingsWithOneUnit_ThenCommonBuildingUnitWasAdded()
        {
            var buildingWasPlannedEvents = Fixture.CreateMany<BuildingWasPlannedV2>(2).ToList();

            var givenPlannedFacts = buildingWasPlannedEvents
                .Select(x => CreateFact(x.BuildingPersistentLocalId, x));

            var command = new MergeBuildings(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                buildingWasPlannedEvents.Select(x => new BuildingPersistentLocalId(x.BuildingPersistentLocalId)),
                Fixture.Create<Provenance>()
            );

            var buildingUnitFacts = new List<Fact>();
            var buildingUnitTransferAssertions = new List<Fact>();
            foreach (var buildingWasPlanned in buildingWasPlannedEvents)
            {
                var buildingUnitWasPlannedV2 = PlanUnit(buildingWasPlanned.BuildingPersistentLocalId);

                buildingUnitFacts.Add(CreateFact(buildingUnitWasPlannedV2.BuildingPersistentLocalId,
                    buildingUnitWasPlannedV2));

                buildingUnitTransferAssertions.Add(CreateFact(command.NewBuildingPersistentLocalId,
                    TransferUnit(command.NewBuildingPersistentLocalId, buildingUnitWasPlannedV2)));
            }

            var expectedPosition = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.MeasuredByGrb).Center;

            Assert(new Scenario()
                .Given(givenPlannedFacts
                    .Concat(RealizePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(MeasurePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(buildingUnitFacts)
                    .ToArray())
                .When(command)
                .Then(
                    CreateFact(command.NewBuildingPersistentLocalId, command.ToBuildingMergerWasRealizedEvent()),
                    buildingUnitTransferAssertions[0],
                    buildingUnitTransferAssertions[1],
                    CreateFact(command.NewBuildingPersistentLocalId,
                        new CommonBuildingUnitWasAddedV2(
                        command.NewBuildingPersistentLocalId,
                        new BuildingUnitPersistentLocalId(1),
                        BuildingUnitStatus.Realized,
                        BuildingUnitPositionGeometryMethod.DerivedFromObject,
                        expectedPosition,
                        false))
                ));
        }

        [Fact]
        public void WithTwoBuildingsWithCommonUnit_ThenCommonBuildingUnitWasAddedAndAddressesWereTransferred()
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

            var buildingUnitFacts = new List<Fact>();
            var buildingUnitTransferAssertions = new List<Fact>();
            foreach (var buildingWasPlanned in buildingWasPlannedEvents)
            {
                var buildingUnitWasPlanned_1 = PlanUnit(buildingWasPlanned.BuildingPersistentLocalId);
                var buildingUnitWasPlanned_2 = PlanUnit(buildingWasPlanned.BuildingPersistentLocalId);

                var commonBuildingUnitWasAdded = AddCommonUnit(buildingWasPlanned.BuildingPersistentLocalId, Fixture.Create<BuildingUnitPersistentLocalId>());

                var commonBuildingUnitAddressWasAttached = AttachCommonUnitAddress(
                    buildingWasPlanned.BuildingPersistentLocalId,
                    commonBuildingUnitWasAdded.BuildingUnitPersistentLocalId);

                buildingUnitFacts.Add(CreateFact(buildingUnitWasPlanned_1.BuildingPersistentLocalId,
                    buildingUnitWasPlanned_1));
                buildingUnitFacts.Add(CreateFact(buildingUnitWasPlanned_2.BuildingPersistentLocalId,
                    buildingUnitWasPlanned_2));
                buildingUnitFacts.Add(CreateFact(buildingUnitWasPlanned_1.BuildingPersistentLocalId,
                    commonBuildingUnitWasAdded));
                buildingUnitFacts.Add(CreateFact(buildingUnitWasPlanned_1.BuildingPersistentLocalId,
                    commonBuildingUnitAddressWasAttached));

                buildingUnitTransferAssertions.Add(CreateFact(command.NewBuildingPersistentLocalId,
                    TransferUnit(command.NewBuildingPersistentLocalId, buildingUnitWasPlanned_1)));

                buildingUnitTransferAssertions.Add(CreateFact(command.NewBuildingPersistentLocalId,
                    TransferUnit(command.NewBuildingPersistentLocalId, buildingUnitWasPlanned_2)));

                buildingUnitTransferAssertions.Add(CreateFact(command.NewBuildingPersistentLocalId,
                    new BuildingUnitAddressWasAttachedV2(
                        command.NewBuildingPersistentLocalId,
                        new BuildingUnitPersistentLocalId(1),
                        new AddressPersistentLocalId(commonBuildingUnitAddressWasAttached.AddressPersistentLocalId))));
            }

            var expectedPosition = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.MeasuredByGrb).Center;

            Assert(new Scenario()
                .Given(givenPlannedFacts
                    .Concat(RealizePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(MeasurePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(buildingUnitFacts)
                    .ToArray())
                .When(command)
                .Then(
                    CreateFact(command.NewBuildingPersistentLocalId, command.ToBuildingMergerWasRealizedEvent()),
                    buildingUnitTransferAssertions[0],
                    buildingUnitTransferAssertions[1],
                    CreateFact(command.NewBuildingPersistentLocalId,
                        new CommonBuildingUnitWasAddedV2(
                            command.NewBuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingUnitStatus.Realized,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            expectedPosition,
                            false)),
                    buildingUnitTransferAssertions[2],
                    buildingUnitTransferAssertions[3],
                    buildingUnitTransferAssertions[4],
                    buildingUnitTransferAssertions[5]
                ));
        }

        private static BuildingUnitWasTransferred TransferUnit(
            int buildingPersistentLocalId,
            BuildingUnitWasPlannedV2 buildingUnitWasPlanned)
        {
            var buildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(buildingUnitWasPlanned.ExtendedWkbGeometry),
                BuildingUnitPositionGeometryMethod.Parse(buildingUnitWasPlanned.GeometryMethod));

            return new BuildingUnitWasTransferred(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                BuildingUnit.Transfer(_ => { },
                    new BuildingPersistentLocalId(buildingPersistentLocalId),
                    new BuildingUnitPersistentLocalId(buildingUnitWasPlanned.BuildingUnitPersistentLocalId),
                    BuildingUnitFunction.Unknown,
                    BuildingUnitStatus.Planned,
                    new List<AddressPersistentLocalId>(),
                    buildingUnitPosition,
                    buildingUnitWasPlanned.HasDeviation),
                new BuildingPersistentLocalId(buildingUnitWasPlanned.BuildingPersistentLocalId),
                buildingUnitPosition);
        }

        private BuildingUnitAddressWasAttachedV2 AttachCommonUnitAddress(
            int buildingPersistentLocalId,
            int buildingUnitPersistentLocalId)
        {
            var commonBuildingUnitAddressWasAttached = new BuildingUnitAddressWasAttachedV2(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitPersistentLocalId),
                Fixture.Create<AddressPersistentLocalId>());
            ((ISetProvenance) commonBuildingUnitAddressWasAttached).SetProvenance(Fixture.Create<Provenance>());
            return commonBuildingUnitAddressWasAttached;
        }

        private BuildingUnitWasPlannedV2 PlanUnit(int buildingPersistentLocalId)
        {
            var e = new BuildingUnitWasPlannedV2(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                new ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary()),
                Fixture.Create<BuildingUnitFunction>(),
                Fixture.Create<bool>());
            ((ISetProvenance) e).SetProvenance(Fixture.Create<Provenance>());
            return e;
        }

        private CommonBuildingUnitWasAddedV2 AddCommonUnit(int buildingPersistentLocalId, int commonBuildingUnitPersistentLocalId)
        {
            var e = new CommonBuildingUnitWasAddedV2(
                new BuildingPersistentLocalId(buildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(commonBuildingUnitPersistentLocalId),
                BuildingUnitStatus.Planned,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                new ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary()),
                hasDeviation: false);
            ((ISetProvenance) e).SetProvenance(Fixture.Create<Provenance>());
            return e;
        }

        private Fact CreateFact(int buildingPersistentLocalId, object @event)
        {
            return new Fact(
                new BuildingStreamId(
                    new BuildingPersistentLocalId(buildingPersistentLocalId)), @event);
        }
    }
}
