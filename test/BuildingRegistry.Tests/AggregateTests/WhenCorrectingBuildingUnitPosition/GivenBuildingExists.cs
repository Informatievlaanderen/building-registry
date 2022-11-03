namespace BuildingRegistry.Tests.AggregateTests.WhenCorrectingBuildingUnitPosition
{
    using System;
    using System.Collections.Generic;
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
    using BuildingRegistry.Legacy;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingGeometryMethod = Building.BuildingGeometryMethod;
    using BuildingId = Building.BuildingId;
    using BuildingStatus = Building.BuildingStatus;
    using BuildingUnitFunction = Building.BuildingUnitFunction;
    using BuildingUnitPositionGeometryMethod = Building.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = Building.BuildingUnitStatus;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

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
                "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3137\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>140285.15277253836 186725.74131567031</gml:pos></gml:Point>";

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

        [Theory]
        [InlineData("NotRealized")]
        [InlineData("Retired")]
        public void WithInvalidBuildingUnitStatus_ThrowsBuildingUnitHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<CorrectBuildingUnitPosition>();

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                new BuildingPersistentLocalId(command.BuildingPersistentLocalId),
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Planned,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingRegistry.Building.Commands.BuildingUnit>
                {
                    new  BuildingRegistry.Building.Commands.BuildingUnit(
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        new PersistentLocalId(command.BuildingUnitPersistentLocalId),
                        BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Parse(status) ?? throw new ArgumentException(),
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        isRemoved: false)
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());

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

            var buildingWasMigrated = new BuildingWasMigrated(
                Fixture.Create<BuildingId>(),
                new BuildingPersistentLocalId(command.BuildingPersistentLocalId),
                Fixture.Create<BuildingPersistentLocalIdAssignmentDate>(),
                BuildingStatus.Planned,
                Fixture.Create<BuildingGeometry>(),
                isRemoved: false,
                new List<BuildingRegistry.Building.Commands.BuildingUnit>
                {
                    new  BuildingRegistry.Building.Commands.BuildingUnit(
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitId>(),
                        new PersistentLocalId(command.BuildingUnitPersistentLocalId),
                        BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                        BuildingRegistry.Legacy.BuildingUnitStatus.Parse(status) ?? throw new ArgumentException(),
                        new List<AddressPersistentLocalId>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingUnitPosition>(),
                        Fixture.Create<BuildingRegistry.Legacy.BuildingGeometry>(),
                        isRemoved: false)
                }
            );
            ((ISetProvenance)buildingWasMigrated).SetProvenance(Fixture.Create<Provenance>());
                
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
                .WithPointPosition("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3137\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
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
        public void WithBuildingUnitPositionOutsideOfBuildingGeometry_ThrowsBuildingUnitOutsideGeometryBuildingException()
        {
            var wrongPointCoordinateX = "666666.77777777777";
            var wrongPointCoordinateY = "777777.66666666666";
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var command = Fixture.Create<CorrectBuildingUnitPosition>()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.AppointedByAdministrator)
                .WithPointPosition("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3137\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
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
        public void WithCommonBuilding_ThrowsBuildingUnitHasInvalidFunctionException()
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var command = Fixture.Create<CorrectBuildingUnitPosition>()
                .WithPersistentLocalId(buildingUnitPersistentLocalId);

            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlanned = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)
                .WithFunction(BuildingUnitFunction.Common);

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
                .WithPointPosition("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/3137\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
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

            var building = new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>()).Create();

            var buildingWasPlannedV2 = new BuildingWasPlannedV2(
                Fixture.Create<BuildingPersistentLocalId>(),
                buildingGeometry.ToExtendedWkbGeometry());
            ;
            ((ISetProvenance)buildingWasPlannedV2).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitWasPlannedV2 = Fixture.Create<BuildingUnitWasPlannedV2>()
                .WithBuildingUnitPersistentLocalId(buildingUnitPersistentLocalId)
                .WithGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);
            ((ISetProvenance)buildingUnitWasPlannedV2).SetProvenance(Fixture.Create<Provenance>());

            var buildingUnitPositionWasCorrected = new BuildingUnitPositionWasCorrected(
                command.BuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                command.PositionGeometryMethod,
                command.Position);
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
            buildingUnit.Status.Should().Be(BuildingUnitStatus.Planned);
            buildingUnit.BuildingUnitPosition.Geometry.ToString().Should().Be(command.Position.ToString());
            buildingUnit.BuildingUnitPosition.GeometryMethod.ToString().Should().Be(command.PositionGeometryMethod.ToString());
            buildingUnit.IsRemoved.Should().BeFalse();
            buildingUnit.LastEventHash.Should().Be(buildingUnitPositionWasCorrected.GetHash());
        }
    }
}
