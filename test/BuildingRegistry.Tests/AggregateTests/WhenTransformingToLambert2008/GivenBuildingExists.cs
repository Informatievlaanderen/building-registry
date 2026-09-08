namespace BuildingRegistry.Tests.AggregateTests.WhenTransformingToLambert2008
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;
    using LegacyBuildingUnitPositionGeometryMethod = BuildingRegistry.Legacy.BuildingUnitPositionGeometryMethod;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Theory]
        [InlineData(nameof(BuildingGeometryMethod.Outlined))]
        [InlineData(nameof(BuildingGeometryMethod.MeasuredByGrb))]
        public void ThenBuildingGeometryCrsWasChanged(string geometryMethod)
        {
            var lambert72 = Lambert72Polygon();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(lambert72, BuildingGeometryMethod.Parse(geometryMethod)))
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()), buildingWasMigrated)
                .When(Command())
                .Then(new Fact(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    new BuildingGeometryCrsWasChanged(
                        Fixture.Create<BuildingPersistentLocalId>(),
                        [],
                        [],
                        ToLambert2008(lambert72),
                        null))));
        }

        /// <summary>
        /// The transformation is not an edit: the geometry is re-expressed, the method it was captured with
        /// is not touched.
        /// </summary>
        [Theory]
        [InlineData(nameof(BuildingGeometryMethod.Outlined))]
        [InlineData(nameof(BuildingGeometryMethod.MeasuredByGrb))]
        public void ThenGeometryMethodIsUnchanged(string geometryMethod)
        {
            var method = BuildingGeometryMethod.Parse(geometryMethod);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(Lambert72Polygon(), method))
                .Build();

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize([buildingWasMigrated]);

            building.TransformToLambert2008();

            building.BuildingGeometry.Method.Should().Be(method);
            ReadGeometry(building.BuildingGeometry.Geometry).SRID.Should().Be(ExtendedWkbGeometry.SridLambert2008);
        }

        [Fact]
        public void WithGeometryAlreadyInLambert2008_ThenNothing()
        {
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(
                    ToLambert2008(Lambert72Polygon()),
                    BuildingGeometryMethod.Outlined))
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()), buildingWasMigrated)
                .When(Command())
                .ThenNone());
        }

        /// <summary>
        /// Running the migrator over a stream twice must be a no-op, not a double transform.
        /// </summary>
        [Fact]
        public void WhenTransformedTwice_ThenSecondRunAppliesNothing()
        {
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(Lambert72Polygon(), BuildingGeometryMethod.Outlined))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    positionGeometryMethod: LegacyBuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: Legacy(Lambert72Point()))
                .Build();

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize([buildingWasMigrated]);

            building.TransformToLambert2008();
            var afterFirstRun = building.GetChanges().Count();

            building.TransformToLambert2008();

            afterFirstRun.Should().Be(2);
            building.GetChanges().Should().HaveCount(afterFirstRun);
        }

        /// <summary>
        /// A derived unit follows the building, and takes the centroid of the transformed geometry rather
        /// than the transform of the old centroid, so a later geometry change or RepairBuilding does not
        /// immediately correct it again. See ADR 0006.
        /// </summary>
        [Fact]
        public void WithDerivedBuildingUnit_ThenPositionIsTheTransformedCenter()
        {
            var lambert72 = Lambert72Polygon();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(lambert72, BuildingGeometryMethod.Outlined))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    buildingUnitPersistentLocalId,
                    positionGeometryMethod: LegacyBuildingUnitPositionGeometryMethod.DerivedFromObject)
                .Build();

            var expectedCenter = new BuildingGeometry(ToLambert2008(lambert72), BuildingGeometryMethod.Outlined).Center;

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()), buildingWasMigrated)
                .When(Command())
                .Then(new Fact(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    new BuildingGeometryCrsWasChanged(
                        Fixture.Create<BuildingPersistentLocalId>(),
                        [buildingUnitPersistentLocalId],
                        [],
                        ToLambert2008(lambert72),
                        expectedCenter))));
        }

        [Fact]
        public void WithAppointedBuildingUnit_ThenItsOwnPositionIsTransformed()
        {
            var lambert72 = Lambert72Polygon();
            var position = Lambert72Point();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(lambert72, BuildingGeometryMethod.Outlined))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    buildingUnitPersistentLocalId,
                    positionGeometryMethod: LegacyBuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: Legacy(position))
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()), buildingWasMigrated)
                .When(Command())
                .Then(
                    new Fact(
                        new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                        new BuildingGeometryCrsWasChanged(
                            Fixture.Create<BuildingPersistentLocalId>(),
                            [],
                            [],
                            ToLambert2008(lambert72),
                            null)),
                    new Fact(
                        new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                        new BuildingUnitPositionCrsWasChanged(
                            Fixture.Create<BuildingPersistentLocalId>(),
                            buildingUnitPersistentLocalId,
                            BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                            ToLambert2008(position, GeometryReferenceSystem.PositionRoundingPrecision)))));
        }

        /// <summary>
        /// Positions are rounded to centimetres, the building geometry is not. See ADR 0006.
        /// </summary>
        [Fact]
        public void ThenPositionsAreRoundedToCentimetresAndTheGeometryIsNot()
        {
            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(Lambert72Polygon(), BuildingGeometryMethod.Outlined))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    positionGeometryMethod: LegacyBuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: Legacy(Lambert72Point()))
                .Build();

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize([buildingWasMigrated]);

            building.TransformToLambert2008();

            var position = ReadGeometry(building.BuildingUnits.Single().BuildingUnitPosition.Geometry);
            position.Coordinates.Should().AllSatisfy(coordinate =>
            {
                coordinate.X.Should().Be(System.Math.Round(coordinate.X, GeometryReferenceSystem.PositionRoundingPrecision));
                coordinate.Y.Should().Be(System.Math.Round(coordinate.Y, GeometryReferenceSystem.PositionRoundingPrecision));
            });

            var geometry = ReadGeometry(building.BuildingGeometry.Geometry);
            geometry.Coordinates.Should().Contain(coordinate =>
                coordinate.X != System.Math.Round(coordinate.X, GeometryReferenceSystem.PositionRoundingPrecision));
        }

        /// <summary>
        /// The transformation has to reach every building the event store holds, or the store is left with
        /// both reference systems forever. It is not an edit, so the removal guard does not apply.
        /// </summary>
        [Fact]
        public void WithRemovedBuilding_ThenBuildingGeometryCrsWasChanged()
        {
            var lambert72 = Lambert72Polygon();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(lambert72, BuildingGeometryMethod.Outlined))
                .WithIsRemoved()
                .Build();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()), buildingWasMigrated)
                .When(Command())
                .Then(new Fact(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    new BuildingGeometryCrsWasChanged(
                        Fixture.Create<BuildingPersistentLocalId>(),
                        [],
                        [],
                        ToLambert2008(lambert72),
                        null))));
        }

        /// <summary>
        /// Same reasoning as for a removed building, and unlike a geometry change, which only reaches
        /// planned and realized units.
        /// </summary>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void WithRetiredOrRemovedBuildingUnit_ThenItsPositionIsTransformed(bool isRemoved)
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(Lambert72Polygon(), BuildingGeometryMethod.Outlined))
                .WithBuildingUnit(
                    BuildingUnitStatus.Retired,
                    buildingUnitPersistentLocalId,
                    positionGeometryMethod: LegacyBuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: Legacy(Lambert72Point()),
                    isRemoved: isRemoved)
                .Build();

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize([buildingWasMigrated]);

            building.TransformToLambert2008();

            building.GetChanges().OfType<BuildingUnitPositionCrsWasChanged>()
                .Should().ContainSingle(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);
        }

        /// <summary>
        /// A position that ends up outside its building after the transformation - a rounding artifact -
        /// is re-derived rather than left outside, exactly as a geometry change does. See ADR 0006.
        /// </summary>
        [Fact]
        public void WithPositionPushedOutsideByTheTransformation_ThenItBecomesDerived()
        {
            var lambert72 = Lambert72Polygon();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var justInside = ExtendedWkbGeometry.Create(GeometryHelper.PointInPolygonPushedOutsideByLambert2008Rounding);

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(lambert72, BuildingGeometryMethod.Outlined))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    buildingUnitPersistentLocalId,
                    positionGeometryMethod: LegacyBuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: Legacy(justInside))
                .Build();

            var newGeometry = new BuildingGeometry(ToLambert2008(lambert72), BuildingGeometryMethod.Outlined);

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize([buildingWasMigrated]);

            // The premise of the test: inside before, outside after.
            building.BuildingGeometry.Contains(justInside).Should().BeTrue();
            newGeometry.Contains(ToLambert2008(justInside, GeometryReferenceSystem.PositionRoundingPrecision))
                .Should().BeFalse();

            building.TransformToLambert2008();

            var geometryEvent = building.GetChanges().OfType<BuildingGeometryCrsWasChanged>().Single();

            geometryEvent.BuildingUnitPersistentLocalIdsWhichBecameDerived
                .Should().Equal((int)buildingUnitPersistentLocalId);
            geometryEvent.ExtendedWkbGeometryBuildingUnits.Should().Be(newGeometry.Center.ToString());
            building.GetChanges().OfType<BuildingUnitPositionCrsWasChanged>().Should().BeEmpty();

            var buildingUnit = building.BuildingUnits.Single();
            buildingUnit.BuildingUnitPosition.GeometryMethod
                .Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject);
            buildingUnit.BuildingUnitPosition.Geometry.Should().Be(newGeometry.Center);
        }

        /// <summary>
        /// A position that was already outside its building before the transformation is not something this
        /// caused, so it is left classified as it is. See ADR 0006.
        /// </summary>
        [Fact]
        public void WithPositionAlreadyOutside_ThenItKeepsItsOwnPosition()
        {
            var outside = ExtendedWkbGeometry.Create(
                new Point(1, 1) { SRID = ExtendedWkbGeometry.SridLambert72 });
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(Lambert72Polygon(), BuildingGeometryMethod.Outlined))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    buildingUnitPersistentLocalId,
                    positionGeometryMethod: LegacyBuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: Legacy(outside))
                .Build();

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize([buildingWasMigrated]);

            building.TransformToLambert2008();

            building.GetChanges().OfType<BuildingGeometryCrsWasChanged>().Single()
                .BuildingUnitPersistentLocalIdsWhichBecameDerived.Should().BeEmpty();
            building.BuildingUnits.Single().BuildingUnitPosition.GeometryMethod
                .Should().Be(BuildingUnitPositionGeometryMethod.AppointedByAdministrator);
        }

        [Fact]
        public void ThenStateWasCorrectlySet()
        {
            var lambert72 = Lambert72Polygon();
            var derivedUnit = Fixture.Create<BuildingUnitPersistentLocalId>();
            var appointedUnit = Fixture.Create<BuildingUnitPersistentLocalId>();
            var position = Lambert72Point();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingGeometry(new BuildingGeometry(lambert72, BuildingGeometryMethod.MeasuredByGrb))
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    derivedUnit,
                    positionGeometryMethod: LegacyBuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithBuildingUnit(
                    BuildingUnitStatus.Realized,
                    appointedUnit,
                    positionGeometryMethod: LegacyBuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                    extendedWkbGeometry: Legacy(position))
                .Build();

            var expectedGeometry = new BuildingGeometry(ToLambert2008(lambert72), BuildingGeometryMethod.MeasuredByGrb);

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            building.Initialize([buildingWasMigrated]);

            building.TransformToLambert2008();

            building.BuildingGeometry.Geometry.Should().Be(expectedGeometry.Geometry);
            building.BuildingGeometry.Method.Should().Be(BuildingGeometryMethod.MeasuredByGrb);

            var derived = building.BuildingUnits.Single(x => x.BuildingUnitPersistentLocalId == derivedUnit);
            derived.BuildingUnitPosition.Geometry.Should().Be(expectedGeometry.Center);
            derived.BuildingUnitPosition.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject);

            var appointed = building.BuildingUnits.Single(x => x.BuildingUnitPersistentLocalId == appointedUnit);
            appointed.BuildingUnitPosition.Geometry.Should()
                .Be(ToLambert2008(position, GeometryReferenceSystem.PositionRoundingPrecision));
            appointed.BuildingUnitPosition.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.AppointedByAdministrator);
        }

        private TransformToLambert2008 Command()
            => new TransformToLambert2008(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<Provenance>());

        private static BuildingRegistry.Legacy.ExtendedWkbGeometry Legacy(ExtendedWkbGeometry geometry)
            => new BuildingRegistry.Legacy.ExtendedWkbGeometry(geometry.ToString());

        private static ExtendedWkbGeometry Lambert72Polygon()
            => ExtendedWkbGeometry.Create(GeometryHelper.ValidPolygon);

        private static ExtendedWkbGeometry Lambert72Point()
            => ExtendedWkbGeometry.Create(GeometryHelper.ValidPointInPolygon);

        private static ExtendedWkbGeometry ToLambert2008(ExtendedWkbGeometry geometry)
            => ExtendedWkbGeometry.Create(
                ReadGeometry(geometry).ToReferenceSystem(ExtendedWkbGeometry.SridLambert2008));

        private static ExtendedWkbGeometry ToLambert2008(ExtendedWkbGeometry geometry, int roundingPrecision)
            => ExtendedWkbGeometry.Create(
                ReadGeometry(geometry).ToReferenceSystem(ExtendedWkbGeometry.SridLambert2008, roundingPrecision));

        private static Geometry ReadGeometry(ExtendedWkbGeometry geometry)
        {
            var extendedWkb = geometry.ToByteArray();

            return WKBReaderFactory.CreateForEwkb(extendedWkb).Read(extendedWkb);
        }
    }
}
