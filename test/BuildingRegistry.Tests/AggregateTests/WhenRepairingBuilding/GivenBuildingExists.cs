namespace BuildingRegistry.Tests.AggregateTests.WhenRepairingBuilding
{
    using System;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Commands;
    using Building.Events;
    using BuildingRegistry.Legacy;
    using Extensions;
    using Fixtures;
    using NetTopologySuite.Geometries;
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
    using WKBReaderFactory = BuildingRegistry.WKBReaderFactory;

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
                new BuildingUnitPosition(ExtendedWkbGeometry.CreateEWkb(WkbWriter.Instance.Write(GeometryHelper.OtherValidPointInPolygon)), BuildingUnitPositionGeometryMethod.DerivedFromObject),
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

        [Fact]
        public void WithUnitsDerivedButCloseToCenter_ThenBuildingAndUnitsWereRepaired()
        {
            var command = Fixture.Create<RepairBuilding>();

            var migrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Realized)
                .Build();

            var reader = WKBReaderFactory.Create();
            reader.HandleSRID = true;

            var centroid = reader.Read(migrated.ExtendedWkbGeometry.ToByteArray()).Centroid;
            var closePosition = new Point(centroid.X - 0.001, centroid.Y + 0.001) { SRID = SystemReferenceId.SridLambert72 };

            migrated.BuildingUnits.Add(new BuildingWasMigrated.BuildingUnit(new BuildingUnit(
                new BuildingUnitId(Fixture.Create<Guid>()),
                new PersistentLocalId(Fixture.Create<int>()),
                BuildingUnitFunction.Unknown,
                BuildingUnitStatus.Realized,
                [],
                new BuildingUnitPosition(ExtendedWkbGeometry.CreateEWkb(WkbWriter.Instance.Write(closePosition)), BuildingUnitPositionGeometryMethod.DerivedFromObject),
                new BuildingGeometry(new ExtendedWkbGeometry(migrated.ExtendedWkbGeometry.ToByteArray()), BuildingRegistry.Legacy.BuildingGeometryMethod.MeasuredByGrb),
                false)));

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>())!,
                    migrated)
                .When(command)
                .ThenNone());
        }

        /// <summary>
        /// A real building after the Lambert 2008 transformation, whose derived unit was given the rounded
        /// centroid 744478.27 698227.85 - 0.9 mm outside the building. The centre now is the nearest grid
        /// point inside, 744478.26 698227.85: exactly a centimetre away, which the distance threshold alone
        /// would skip.
        ///
        /// Whether it does depends on floating point: 744478.27 - 744478.26 comes out at 0.010000000009, but
        /// 744478.07 - 744478.06 at 0.0099999999. The second case is the same building moved 20 cm west, so
        /// its step is one the threshold alone did skip.
        /// </summary>
        [Theory]
        [InlineData(RealBuilding, 744478.27, 744478.26)]
        [InlineData(RealBuildingMovedTwentyCentimetresWest, 744478.07, 744478.06)]
        public void WithUnitDerivedButOutsideTheBuildingByLessThanACentimetre_ThenTheUnitWasRepaired(
            string buildingWkt,
            double positionOutsideX,
            double repairedPositionX)
        {
            var command = Fixture.Create<RepairBuilding>();
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(Fixture.Create<int>());

            var polygon = new NetTopologySuite.IO.WKTReader(
                    new NetTopologySuite.NtsGeometryServices(new PrecisionModel(), SystemReferenceId.SridLambert2008))
                .Read(buildingWkt);
            var buildingGeometry = new BuildingRegistry.Building.BuildingGeometry(
                BuildingRegistry.Building.ExtendedWkbGeometry.Create(polygon),
                BuildingGeometryMethod.Outlined);

            var migrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingStatus(BuildingStatus.Realized)
                .WithBuildingGeometry(buildingGeometry)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    buildingUnitPersistentLocalId,
                    positionGeometryMethod: BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .Build();

            var positionOutsideTheBuilding = BuildingRegistry.Building.ExtendedWkbGeometry.Create(
                new Point(positionOutsideX, 698227.85) { SRID = SystemReferenceId.SridLambert2008 });
            var positionWasCorrected = new BuildingUnitPositionWasCorrected(
                command.BuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject,
                positionOutsideTheBuilding);
            ((ISetProvenance)positionWasCorrected).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(command.BuildingPersistentLocalId)!,
                    migrated,
                    positionWasCorrected)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(command.BuildingPersistentLocalId)!,
                    new BuildingUnitPositionWasCorrected(
                        command.BuildingPersistentLocalId,
                        buildingUnitPersistentLocalId,
                        BuildingRegistry.Building.BuildingUnitPositionGeometryMethod.DerivedFromObject,
                        BuildingRegistry.Building.ExtendedWkbGeometry.CreatePosition(
                            new Point(repairedPositionX, 698227.85) { SRID = SystemReferenceId.SridLambert2008 })))));
        }

        private const string RealBuilding =
            "POLYGON((744480.1184530781 698229.8643112164,744479.9440770152 698237.9898144752,744472.9558250407 698237.9973120121,744472.9069092406 698223.7407438857,744478.4328983055 698223.7216822859,744478.4125888565 698217.8035041029,744483.4127542111 698217.8060764885,744483.8866076873 698217.8063242519,744483.7962069352 698219.2044500075,744485.3499138679 698219.3161202399,744484.3534088014 698226.7939782962,744483.3100937849 698226.7290878696,744483.2841918147 698227.1289546471,744478.2805655755 698227.1690695966,744478.2367449788 698229.777818243,744480.1184530781 698229.8643112164))";

        private const string RealBuildingMovedTwentyCentimetresWest =
            "POLYGON((744479.9184530781 698229.8643112164,744479.7440770152 698237.9898144752,744472.7558250407 698237.9973120121,744472.7069092407 698223.7407438857,744478.2328983055 698223.7216822859,744478.2125888566 698217.8035041029,744483.2127542112 698217.8060764885,744483.6866076874 698217.8063242519,744483.5962069352 698219.2044500075,744485.1499138679 698219.3161202399,744484.1534088014 698226.7939782962,744483.110093785 698226.7290878696,744483.0841918148 698227.1289546471,744478.0805655755 698227.1690695966,744478.0367449789 698229.777818243,744479.9184530781 698229.8643112164))";
    }
}
