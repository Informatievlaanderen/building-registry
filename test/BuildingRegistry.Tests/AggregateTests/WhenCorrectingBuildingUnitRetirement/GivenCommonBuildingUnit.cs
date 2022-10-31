namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitRetirement
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using BuildingRegistry.Legacy;
    using Extensions;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingId = Building.BuildingId;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnit = Building.Commands.BuildingUnit;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;

    public class GivenCommonBuildingUnit : BuildingRegistryTest
    {
        public GivenCommonBuildingUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithRetiredCommonBuilding_ThenCommonBuildingBecomesRealized()
        {
            var command = Fixture.Create<CorrectBuildingUnitRetirement>();

            var commonBuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitToCorrect = new BuildingUnit(
                Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                new PersistentLocalId(command.BuildingUnitPersistentLocalId),
                BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                new List<AddressPersistentLocalId>(),
                Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                isRemoved: false);
            var bu2 = new BuildingUnit(
                Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                new PersistentLocalId(123),
                BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                new List<AddressPersistentLocalId>(),
                Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                isRemoved: false);
            var commonBuildingUnit = new BuildingUnit(
                Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                new PersistentLocalId(commonBuildingUnitPersistentLocalId),
                BuildingRegistry.Legacy.BuildingUnitFunction.Common,
                BuildingRegistry.Legacy.BuildingUnitStatus.Retired,
                new List<AddressPersistentLocalId>(),
                Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(), // Values are ignored here because common function position
                Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),     // is always derrived from object with (building center position)
                isRemoved: false);

            var buildingGeometry = Fixture.Create<BuildingGeometry>();
            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                command.BuildingPersistentLocalId,
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Realized,
                buildingGeometry,
                isRemoved: false,
                new List<BuildingUnit>
                {
                   buildingUnitToCorrect, bu2, commonBuildingUnit
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRetiredToRealized(
                            command.BuildingPersistentLocalId,
                            command.BuildingUnitPersistentLocalId,
                            buildingUnitToCorrect.BuildingUnitPosition.Geometry)),
                    new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                        new BuildingUnitWasCorrectedFromRetiredToRealized(
                            command.BuildingPersistentLocalId,
                            commonBuildingUnitPersistentLocalId,
                            buildingGeometry.Center)) // Common building unit position is always center
                    ));
        }
    }
}
