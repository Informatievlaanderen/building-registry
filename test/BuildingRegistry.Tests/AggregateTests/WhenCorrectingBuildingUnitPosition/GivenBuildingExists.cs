namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitPosition
{
    using System.Linq;
    using Api.BackOffice.Abstractions.Building;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingUnitFunction = BuildingRegistry.Legacy.BuildingUnitFunction;
    using BuildingUnitStatus = BuildingRegistry.Legacy.BuildingUnitStatus;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void WithNotAppointedByAdministrator_ThenPositionIsCorrectedToCenter()
        {
            var position =
                "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>140285.15277253836 186725.74131567031</gml:pos></gml:Point>";

            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var command = Fixture.Create<CorrectBuildingUnitPosition>()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithPointPosition(position)
                .WithPersistentLocalId(buildingUnitPersistentLocalId);

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);
            var buildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(buildingWasPlanned.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasPlanned,
                    buildingUnitWasPlanned)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitPositionWasCorrected(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.PositionGeometryMethod,
                        buildingGeometry.Center))));
        }

        /// <summary>
        /// A correction that corrects nothing applies nothing. Without this the unit got a
        /// <see cref="BuildingUnitPositionWasCorrected"/> - a new version, a syndication entry, a Kafka
        /// message - for an edit that changed neither the position nor how it was derived.
        /// </summary>
        [Fact]
        public void WithTheDerivedPositionTheUnitAlreadyHas_ThenNone()
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(buildingWasPlanned.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)
                .WithPosition(new BuildingUnitPosition(
                    buildingGeometry.Center,
                    BuildingUnitPositionGeometryMethod.DerivedFromObject));

            var command = Fixture.Create<CorrectBuildingUnitPosition>()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithPointPosition(null)
                .WithPersistentLocalId(buildingUnitPersistentLocalId);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasPlanned,
                    buildingUnitWasPlanned)
                .When(command)
                .ThenNone());
        }

        [Fact]
        public void WithTheAppointedPositionTheUnitAlreadyHas_ThenNone()
        {
            // Inside the building, unlike the point the derived tests use: the position is only ignored when
            // the method is DerivedFromObject, so an appointed one still has to pass the Contains guard that
            // runs before the no-op check.
            var position = GeometryHelper.GmlPointGeometry;

            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)
                .WithPosition(new BuildingUnitPosition(
                    position.ToExtendedWkbPosition(),
                    BuildingUnitPositionGeometryMethod.AppointedByAdministrator));

            var command = Fixture.Create<CorrectBuildingUnitPosition>()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                .WithPointPosition(position)
                .WithPersistentLocalId(buildingUnitPersistentLocalId);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    Fixture.Create<BuildingWasPlannedV2>(),
                    buildingUnitWasPlanned)
                .When(command)
                .ThenNone());
        }

        /// <summary>
        /// Both halves of the position count. Appointing the coordinates a derived unit already sits on is a
        /// real correction - the unit stops following its building - so it is not a no-op.
        /// </summary>
        [Fact]
        public void WithTheSameCoordinatesButAppointedInsteadOfDerived_ThenBuildingUnitPositionWasCorrected()
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(buildingWasPlanned.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)
                .WithPosition(new BuildingUnitPosition(
                    buildingGeometry.Center,
                    BuildingUnitPositionGeometryMethod.DerivedFromObject));

            var command = Fixture.Create<CorrectBuildingUnitPosition>()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                .WithPointPosition(null)
                .WithPersistentLocalId(buildingUnitPersistentLocalId);
            command = new CorrectBuildingUnitPosition(
                command.BuildingPersistentLocalId,
                command.BuildingUnitPersistentLocalId,
                command.PositionGeometryMethod,
                buildingGeometry.Center,
                command.Provenance);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasPlanned,
                    buildingUnitWasPlanned)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitPositionWasCorrected(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                        buildingGeometry.Center))));
        }

        [Theory]
        [InlineData("NotRealized")]
        [InlineData("Retired")]
        public void WithInvalidBuildingUnitStatus_ThenThrowsBuildingUnitHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<CorrectBuildingUnitPosition>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingUnit(BuildingUnitStatus.Parse(status)!.Value,
                    command.BuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
                .Build();

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(new BuildingPersistentLocalId(command.BuildingPersistentLocalId)),
                    buildingWasMigrated)
                .When(command)
                .Throws(new BuildingUnitHasInvalidStatusException()));
        }

        [Theory]
        [InlineData("Realized")]
        [InlineData("Planned")]
        public void WithValidBuildingUnitStatus_ThenBuildingUnitPositionWasCorrected(string status)
        {
            var command = Fixture.Create<CorrectBuildingUnitPosition>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
                .WithBuildingPersistentLocalId(command.BuildingPersistentLocalId)
                .WithBuildingStatus(BuildingStatus.Planned)
                .WithBuildingUnit(
                    BuildingUnitStatus.Parse(status)!.Value,
                    command.BuildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown)
                .Build();

            var buildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(buildingWasMigrated.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasMigrated)
                .When(command)
                .Then(new Fact(
                    new BuildingStreamId(new BuildingPersistentLocalId(command.BuildingPersistentLocalId)),
                    new BuildingUnitPositionWasCorrected(
                        new BuildingPersistentLocalId(command.BuildingPersistentLocalId),
                        new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                        BuildingUnitPositionGeometryMethod.DerivedFromObject,
                        buildingGeometry.Center))));
        }

        [Fact]
        public void WithBuildingUnitPositionInsideOfBuildingGeometry_ThenBuildingUnitPositionWasCorrected()
        {
            var correctPointCoordinateX = "140285.15277253836";
            var correctPointCoordinateY = "186725.74131567031";
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var command = Fixture.Create<CorrectBuildingUnitPosition>()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                .WithPointPosition(
                    "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                    $"<gml:pos>{correctPointCoordinateX} {correctPointCoordinateY}</gml:pos></gml:Point>")
                .WithPersistentLocalId(buildingUnitPersistentLocalId);

            var buildingGeometry = "" +
                                   "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                                   "<gml:exterior>" +
                                   "<gml:LinearRing>" +
                                   "<gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList>" +
                                   "</gml:LinearRing>" +
                                   "</gml:exterior>" +
                                   "</gml:Polygon>";
            var buildingWasPlanned = new BuildingWasPlannedV2(
                Fixture.Create<BuildingPersistentLocalId>(),
                buildingGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)buildingWasPlanned).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasPlanned,
                    buildingUnitWasPlanned)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitPositionWasCorrected(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.PositionGeometryMethod,
                        command.Position!))));
        }

        [Fact]
        public void WithBuildingUnitPositionOutsideOfBuildingGeometry_ThenThrowsBuildingUnitOutsideGeometryBuildingException()
        {
            var wrongPointCoordinateX = "666666.77777777777";
            var wrongPointCoordinateY = "777777.66666666666";
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var command = Fixture.Create<CorrectBuildingUnitPosition>()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                .WithPointPosition(
                    "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                    $"<gml:pos>{wrongPointCoordinateX} {wrongPointCoordinateY}</gml:pos></gml:Point>")
                .WithPersistentLocalId(buildingUnitPersistentLocalId);

            var buildingGeometry = "" +
                                   "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                                   "<gml:exterior>" +
                                   "<gml:LinearRing>" +
                                   "<gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList>" +
                                   "</gml:LinearRing>" +
                                   "</gml:exterior>" +
                                   "</gml:Polygon>";
            var buildingWasPlanned = new BuildingWasPlannedV2(
                Fixture.Create<BuildingPersistentLocalId>(),
                buildingGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)buildingWasPlanned).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    @buildingWasPlanned,
                    buildingUnitWasPlanned)
                .When(command)
                .Throws(new BuildingUnitPositionIsOutsideBuildingGeometryException()));
        }

        [Fact]
        public void WithCommonBuilding_ThenThrowsBuildingUnitHasInvalidFunctionException()
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var command = Fixture.Create<CorrectBuildingUnitPosition>()
                .WithPersistentLocalId(buildingUnitPersistentLocalId);

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)
                .WithFunction(BuildingRegistry.Building.BuildingUnitFunction.Common);

            Assert(new Scenario()
                .Given(
                    new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasPlanned,
                    buildingUnitWasPlanned)
                .When(command)
                .Throws(new BuildingUnitHasInvalidFunctionException()));
        }

        [Fact]
        public void ThenStateWasCorrectlySet()
        {
            var correctPointCoordinateX = "140285.15277253836";
            var correctPointCoordinateY = "186725.74131567031";
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();
            var command = Fixture.Create<CorrectBuildingUnitPosition>()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                .WithPointPosition(
                    "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                    $"<gml:pos>{correctPointCoordinateX} {correctPointCoordinateY}</gml:pos></gml:Point>")
                .WithPersistentLocalId(buildingUnitPersistentLocalId);

            var buildingGeometry = "" +
                                   "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                                   "<gml:exterior>" +
                                   "<gml:LinearRing>" +
                                   "<gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList>" +
                                   "</gml:LinearRing>" +
                                   "</gml:exterior>" +
                                   "</gml:Polygon>";

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();

            var buildingWasPlannedV2 = new BuildingWasPlannedV2(
                Fixture.Create<BuildingPersistentLocalId>(),
                buildingGeometry.ToExtendedWkbGeometry());

            ((ISetProvenance)buildingWasPlannedV2).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)
                .WithGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);
            ((ISetProvenance)buildingUnitWasPlannedV2).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitPositionWasCorrected = new BuildingUnitPositionWasCorrected(
                command.BuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                command.PositionGeometryMethod,
                command.Position!);
            ((ISetProvenance)buildingUnitPositionWasCorrected).SetProvenance(Fixture.Create<Provenance>());

            building.Initialize(new object[]
            {
                buildingWasPlannedV2,
                buildingUnitWasPlannedV2,
                buildingUnitPositionWasCorrected
            });

            building.BuildingUnits.Should().NotBeEmpty();
            building.BuildingUnits.Count.Should().Be(1);
            var buildingUnit = building.BuildingUnits.First();
            buildingUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Planned);
            buildingUnit.BuildingUnitPosition.Geometry.ToString().Should().Be(command.Position!.ToString());
            buildingUnit.BuildingUnitPosition.GeometryMethod.ToString().Should().Be(command.PositionGeometryMethod.ToString());
            buildingUnit.IsRemoved.Should().BeFalse();
            buildingUnit.LastEventHash.Should().Be(buildingUnitPositionWasCorrected.GetHash());
        }
    }
}
