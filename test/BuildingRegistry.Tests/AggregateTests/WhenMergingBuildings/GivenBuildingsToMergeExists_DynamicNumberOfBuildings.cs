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

    public partial class GivenBuildingsToMergeExists
    {
        [Fact]
        public void WithBuildingsWithBuildingUnits_ThenBuildingMergerWasRealizedAndUnitsTransferred()
        {
            var buildingWasPlannedEvents = Fixture.CreateMany<BuildingWasPlannedV2>(_random.Next(2, 20)).ToList();

            var givenPlannedFacts = buildingWasPlannedEvents
                .Select(x =>
                    new Fact(new BuildingStreamId(new BuildingPersistentLocalId(x.BuildingPersistentLocalId)), x));

            var command = new MergeBuildings(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                buildingWasPlannedEvents.Select(x => new BuildingPersistentLocalId(x.BuildingPersistentLocalId)),
                Fixture.Create<Provenance>()
            );

            var givenBuildingUnitFacts = new List<Fact>();
            var buildingUnitTransferAssertions = new List<Fact>();
            var newCommonUnitCreated = false;
            var commonBuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();
            var numberOfNewUnitsAdded = 0;

            foreach (var buildingWasPlanned in buildingWasPlannedEvents)
            {
                var numberOfUnits = _random.Next(0, 5);
                CommonBuildingUnitWasAddedV2? commonBuildingUnitWasAdded = null;
                var commonBuildingUnitAddressesWasAttached = new List<BuildingUnitAddressWasAttachedV2>();

                for (var i = 0; i < numberOfUnits; i++)
                {
                    numberOfNewUnitsAdded++;
                    var buildingUnitWasPlanned = PlanUnit(buildingWasPlanned.BuildingPersistentLocalId);
                    givenBuildingUnitFacts.Add(CreateFact(buildingUnitWasPlanned.BuildingPersistentLocalId, buildingUnitWasPlanned));
                    buildingUnitTransferAssertions.Add(CreateFact(command.NewBuildingPersistentLocalId,
                        TransferUnit(command.NewBuildingPersistentLocalId, buildingUnitWasPlanned)));

                    if (i == 1 &&
                        commonBuildingUnitWasAdded is null) // create CommonUnit when second unit is added AND commonUnit is not already created
                    {
                        commonBuildingUnitWasAdded = AddCommonUnit(buildingWasPlanned.BuildingPersistentLocalId, commonBuildingUnitPersistentLocalId);
                        givenBuildingUnitFacts.Add(CreateFact(buildingUnitWasPlanned.BuildingPersistentLocalId, commonBuildingUnitWasAdded));

                        for (var addressIndex = 0; addressIndex < _random.Next(0, 3); addressIndex++)
                        {
                            var commonBuildingUnitAddressWasAttached = AttachBuildingUnitAddress(
                                buildingWasPlanned.BuildingPersistentLocalId, commonBuildingUnitPersistentLocalId);
                            givenBuildingUnitFacts.Add(CreateFact(buildingUnitWasPlanned.BuildingPersistentLocalId,
                                commonBuildingUnitAddressWasAttached));

                            commonBuildingUnitAddressesWasAttached.Add(commonBuildingUnitAddressWasAttached);
                        }
                    }

                    if (numberOfNewUnitsAdded != 2 || newCommonUnitCreated)
                        continue;

                    buildingUnitTransferAssertions.Add(CreateFact(command.NewBuildingPersistentLocalId,
                        new CommonBuildingUnitWasAddedV2(
                            command.NewBuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            BuildingUnitStatus.Realized,
                            BuildingUnitPositionGeometryMethod.DerivedFromObject,
                            NewBuildingCenter,
                            false)));

                    newCommonUnitCreated = true;
                }

                if (numberOfUnits < 2 || commonBuildingUnitWasAdded is null)
                    continue;

                foreach (var addressWasAttachedV2 in commonBuildingUnitAddressesWasAttached)
                {
                    buildingUnitTransferAssertions.Add(CreateFact(command.NewBuildingPersistentLocalId,
                        new BuildingUnitAddressWasAttachedV2(
                            command.NewBuildingPersistentLocalId,
                            new BuildingUnitPersistentLocalId(1),
                            new AddressPersistentLocalId(addressWasAttachedV2.AddressPersistentLocalId))));
                }
            }

            var expectedFacts = new List<Fact>
            {
                CreateFact(command.NewBuildingPersistentLocalId, command.ToBuildingMergerWasRealizedEvent())
            };
            expectedFacts.AddRange(buildingUnitTransferAssertions);

            Assert(new Scenario()
                .Given(givenPlannedFacts
                    .Concat(RealizePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(MeasurePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(givenBuildingUnitFacts)
                    .ToArray())
                .When(command)
                .Then(expectedFacts.ToArray()));
        }
    }
}
