namespace BuildingRegistry.Tests.AggregateTests.WhenRepairingBuilding
{
    using System;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Commands;
    using Building.Events;
    using BuildingRegistry.Legacy;
    using Extensions;
    using Fixtures;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = BuildingRegistry.Legacy.BuildingGeometry;
    using BuildingGeometryMethod = Building.BuildingGeometryMethod;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnit = Building.Commands.BuildingUnit;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitId = BuildingRegistry.Legacy.BuildingUnitId;
    using BuildingUnitPosition = BuildingRegistry.Legacy.BuildingUnitPosition;
    using BuildingUnitPositionGeometryMethod = BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;
    using ExtendedWkbGeometry = BuildingRegistry.Legacy.ExtendedWkbGeometry;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithUnitsDerivedButNotInCenter_ThenBuildingAndUnitsWereRepaired()
        {
            var command = Fixture.Create<RepairBuilding>();

            var migrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Realized)
                .Build();

            migrated.BuildingUnits.Add(new BuildingWasMigrated.BuildingUnit(new BuildingUnit(
                new BuildingUnitId(Fixture.Create<Guid>()),
                new PersistentLocalId(Fixture.Create<int>()),
                BuildingUnitFunction.Unknown,
                BuildingUnitStatus.Realized,
                [],
                new BuildingUnitPosition(ExtendedWkbGeometry.CreateEWkb(GeometryHelper.OtherValidPointInPolygon.AsBinary()), BuildingUnitPositionGeometryMethod.DerivedFromObject),
                new BuildingGeometry(new ExtendedWkbGeometry(migrated.ExtendedWkbGeometry.ToByteArray()), BuildingRegistry.Legacy.BuildingGeometryMethod.MeasuredByGrb),
                false)));

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>())!,
                    migrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId)!,
                    new BuildingUnitPositionWasCorrected(
                        command.BuildingPersistentLocalId,
                        new BuildingUnitPersistentLocalId(migrated.BuildingUnits[0].BuildingUnitPersistentLocalId),
                        BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject,
                        new BuildingRegistry.Building.BuildingGeometry(BuildingRegistry.Building.ExtendedWkbGeometry.CreateEWkb(migrated.ExtendedWkbGeometry.ToByteArray()), BuildingGeometryMethod.MeasuredByGrb).Center))));
        }
    }
}
