namespace BuildingRegistry.Tests.AggregateTests.WhenPlanningBuildingUnit
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
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public void ThenStateWasCorrectlySet()
        {
            var command = Fixture.Create<PlanBuildingUnit>().WithDeviation(false);

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();

            var buildingWasPlannedV2 = Fixture.Create<BuildingWasPlannedV2>();
            ((ISetProvenance)buildingWasPlannedV2).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>();
            ((ISetProvenance)buildingUnitWasPlannedV2).SetProvenance(Fixture.Create<Provenance>());

            building.Initialize(new object[]
            {
                buildingWasPlannedV2,
                buildingUnitWasPlannedV2
            });

            building.BuildingUnits.Should().NotBeEmpty();
            building.BuildingUnits.Count.Should().Be(1);
            var buildingUnit = building.BuildingUnits.First();
            buildingUnit.Status.Should().Be(BuildingUnitStatus.Planned);
            buildingUnit.BuildingUnitPosition.Geometry.ToString().Should().Be(command.Position.ToString());
            buildingUnit.BuildingUnitPosition.GeometryMethod.ToString().Should().Be(command.PositionGeometryMethod.ToString());
            buildingUnit.Function.Should().Be(command.Function);
            buildingUnit.HasDeviation.Should().BeFalse();
            buildingUnit.IsRemoved.Should().BeFalse();
            buildingUnit.LastEventHash.Should().Be(building.LastEventHash);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3137\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                    "<gml:pos>140285.15277253836 186725.74131567031</gml:pos></gml:Point>")]
        public void WithNotAppointedByAdministrator_ThenPositionIsCenter(string position)
        {
            var command = Fixture.Create<PlanBuildingUnit>()
                .WithDeviation(false)
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject)
                .WithPointPosition(position);

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(buildingWasPlanned.ExtendedWkbGeometry),
                BuildingGeometryMethod.Outlined);

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasPlanned)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasPlannedV2(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.PositionGeometryMethod,
                        buildingGeometry.Center,
                        command.Function,
                        hasDeviation: false))));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WithAppointedByAdministratorAndNoPosition_ThrowBuildingUnitOutsideGeometryBuildingException(string position)
        {
            var command = Fixture.Create<PlanBuildingUnit>()
                .WithDeviation(false)
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                .WithPointPosition(position);

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasPlanned)
                .When(command)
                .Throws(new BuildingUnitOutsideGeometryBuildingException()));
        }

        [Fact]
        public void WithBuildingUnitPositionInsideOfBuildingGeometry_ThenBuildingUnitWasPlanned()
        {
            var correctPointCoordinateX = "140285.15277253836";
            var correctPointCoordinateY = "186725.74131567031";

            var command = Fixture.Create<PlanBuildingUnit>()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                .WithPointPosition("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3137\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                                   $"<gml:pos>{correctPointCoordinateX} {correctPointCoordinateY}</gml:pos></gml:Point>")
                .WithDeviation(false);

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

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    buildingWasPlanned)
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.BuildingPersistentLocalId),
                    new BuildingUnitWasPlannedV2(
                        command.BuildingPersistentLocalId,
                        command.BuildingUnitPersistentLocalId,
                        command.PositionGeometryMethod,
                        command.Position!,
                        command.Function,
                        false))));
        }

        [Fact]
        public void WithBuildingUnitPositionOutsideOfBuildingGeometry_ThrowBuildingUnitOutsideGeometryBuildingException()
        {
            var wrongPointCoordinateX = "666666.77777777777";
            var wrongPointCoordinateY = "777777.66666666666";

            var command = Fixture.Create<PlanBuildingUnit>()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                .WithPointPosition("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3137\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                                   $"<gml:pos>{wrongPointCoordinateX} {wrongPointCoordinateY}</gml:pos></gml:Point>");

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

            Assert(new Scenario()
                .Given(new BuildingStreamId(Fixture.Create<BuildingPersistentLocalId>()),
                    @buildingWasPlanned)
                .When(command)
                .Throws(new BuildingUnitOutsideGeometryBuildingException()));
        }
    }
}
