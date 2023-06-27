namespace BuildingRegistry.Tests.AggregateTests.WhenMergingBuildings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Datastructures;
    using Building.Events;
    using Building.Exceptions;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public partial class GivenBuildingsToMergeExists : BuildingRegistryTest
    {
        private readonly Random _random;

        public GivenBuildingsToMergeExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _random = new Random();
        }

        [Fact]
        public void WithOnlyOneBuilding_ThenBuildingMergerNeedsMoreThanOneBuildingExceptionIsThrown()
        {
            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>();

            var givenPlannedFacts = new List<Fact>
            {
                new Fact(new BuildingStreamId(new BuildingPersistentLocalId(buildingWasPlanned.BuildingPersistentLocalId)),
                    buildingWasPlanned)
            };

            var command = new MergeBuildings(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                new List<BuildingPersistentLocalId>
                    { new BuildingPersistentLocalId(buildingWasPlanned.BuildingPersistentLocalId) },
                Fixture.Create<Provenance>()
            );

            Assert(new Scenario()
                .Given(givenPlannedFacts
                    .Concat(RealizePlannedBuildingsEvents(new List<BuildingWasPlannedV2> { buildingWasPlanned }))
                    .Concat(MeasurePlannedBuildingsEvents(new List<BuildingWasPlannedV2> { buildingWasPlanned }))
                    .ToArray())
                .When(command)
                .Throws(new BuildingMergerNeedsMoreThanOneBuildingException()));
        }

        [Fact]
        public void WithMoreThanTwentyBuildings_ThenBuildingMergerHasTooManyBuildingsExceptionIsThrown()
        {
            var buildingWasPlannedEvents = Fixture.CreateMany<BuildingWasPlannedV2>(21).ToList();

            var givenPlannedFacts = buildingWasPlannedEvents
                .Select(x =>
                    new Fact(new BuildingStreamId(new BuildingPersistentLocalId(x.BuildingPersistentLocalId)), x));

            var command = new MergeBuildings(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                buildingWasPlannedEvents.Select(x => new BuildingPersistentLocalId(x.BuildingPersistentLocalId)),
                Fixture.Create<Provenance>()
            );

            Assert(new Scenario()
                .Given(givenPlannedFacts
                    .Concat(RealizePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(MeasurePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .ToArray())
                .When(command)
                .Throws(new BuildingMergerHasTooManyBuildingsException()));
        }

        [Fact]
        public void WithPointAsGeometry_ThenInvalidPolygonExceptionWasThrown()
        {
            var buildingWasPlannedEvents = Fixture.CreateMany<BuildingWasPlannedV2>(_random.Next(2, 20)).ToList();
            Fixture.Customize(new WithValidPoint());

            var givenPlannedFacts = buildingWasPlannedEvents
                .Select(x =>
                    new Fact(new BuildingStreamId(new BuildingPersistentLocalId(x.BuildingPersistentLocalId)), x));

            var command = new MergeBuildings(
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<ExtendedWkbGeometry>(),
                buildingWasPlannedEvents.Select(x => new BuildingPersistentLocalId(x.BuildingPersistentLocalId)),
                Fixture.Create<Provenance>()
            );

            Assert(new Scenario()
                .Given(givenPlannedFacts
                    .Concat(RealizePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(MeasurePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .ToArray())
                .When(command)
                .Throws(new PolygonIsInvalidException()));
        }

        [Fact]
        public void WithInvalidStatus_ThenThrowsBuildingToMergeHasInvalidStatusException()
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

            Assert(new Scenario()
                .Given(givenPlannedFacts
                    .Concat(RealizePlannedBuildingsEvents(buildingWasPlannedEvents).Skip(1)) // Skip realizing first in list
                    .Concat(MeasurePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .ToArray())
                .When(command)
                .Throws(new BuildingToMergeHasInvalidStatusException()));
        }

        [Fact]
        public void WithInvalidGeometryMethod_ThenThrowsBuildingToMergeHasInvalidGeometryMethodException()
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

            Assert(new Scenario()
                .Given(givenPlannedFacts
                    .Concat(RealizePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(MeasurePlannedBuildingsEvents(buildingWasPlannedEvents).Skip(1)) // Skip measuring first in list
                    .ToArray())
                .When(command)
                .Throws(new BuildingToMergeHasInvalidGeometryMethodException()));
        }

        [Fact]
        public void WithNoBuildingUnits_ThenBuildingMergerWasRealized()
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

            Assert(new Scenario()
                .Given(givenPlannedFacts
                    .Concat(RealizePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .Concat(MeasurePlannedBuildingsEvents(buildingWasPlannedEvents))
                    .ToArray())
                .When(command)
                .Then(new Fact(new BuildingStreamId(command.NewBuildingPersistentLocalId),
                    command.ToBuildingMergerWasRealizedEvent())));
        }

        [Fact]
        public void StateCheck()
        {
            var command = Fixture.Create<MergeBuildings>();
            //TODO: add multiple units and addresses

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            building.Initialize(
                new List<object>
                {
                    new UnplannedBuildingWasRealizedAndMeasured(
                        buildingPersistentLocalId,
                        Fixture.Create<ExtendedWkbGeometry>()),

                    new BuildingUnitWasPlannedV2(
                        buildingPersistentLocalId,
                        Fixture.Create<BuildingUnitPersistentLocalId>(),
                        BuildingUnitPositionGeometryMethod.AppointedByAdministrator,
                        new ExtendedWkbGeometry(GeometryHelper.ValidPointInPolygon.AsBinary()),
                        BuildingUnitFunction.Unknown,
                        true)
                });

            // Act
            var result = Building.MergeBuildings(
                new BuildingFactory(NoSnapshotStrategy.Instance),
                Mock.Of<IAddCommonBuildingUnit>(),
                command.NewBuildingPersistentLocalId,
                command.NewExtendedWkbGeometry,
                new List<Building>
                {
                    building,
                    Building.RealizeAndMeasureUnplannedBuilding(
                        new BuildingFactory(NoSnapshotStrategy.Instance),
                        Fixture.Create<BuildingPersistentLocalId>(),
                        Fixture.Create<ExtendedWkbGeometry>(),
                        Fixture.Create<BuildingGrbData>())
                });

            // Assert
            result.Should().NotBeNull();
            result.BuildingPersistentLocalId.Should().Be(command.NewBuildingPersistentLocalId);
            result.BuildingStatus.Should().Be(BuildingStatus.Realized);
            result.BuildingGeometry.Geometry.Should().Be(command.NewExtendedWkbGeometry);
            result.BuildingGeometry.Method.Should().Be(BuildingGeometryMethod.MeasuredByGrb);

            //TODO: extend assertions
            result.BuildingUnits.Should().HaveCount(1);
        }

        private List<Fact> RealizePlannedBuildingsEvents(List<BuildingWasPlannedV2> buildingWasPlannedEvents)
        {
            var buildingsWereRealized = new List<BuildingWasRealizedV2>();
            foreach (var planned in buildingWasPlannedEvents)
            {
                var buildingWasRealizedV2 =
                    new BuildingWasRealizedV2(new BuildingPersistentLocalId(planned.BuildingPersistentLocalId));
                ((ISetProvenance)buildingWasRealizedV2).SetProvenance(Fixture.Create<Provenance>());

                buildingsWereRealized.Add(buildingWasRealizedV2);
            }

            return buildingsWereRealized.Select(x =>
                new Fact(new BuildingStreamId(new BuildingPersistentLocalId(x.BuildingPersistentLocalId)), x)).ToList();
        }

        private List<Fact> MeasurePlannedBuildingsEvents(List<BuildingWasPlannedV2> buildingWasPlannedEvents)
        {
            var buildingsWereMeasured = new List<BuildingWasMeasured>();
            foreach (var planned in buildingWasPlannedEvents)
            {
                var buildingWasMeasured =
                    new BuildingWasMeasured(
                        new BuildingPersistentLocalId(planned.BuildingPersistentLocalId),
                        new List<BuildingUnitPersistentLocalId>(),
                        new List<BuildingUnitPersistentLocalId>(),
                        Fixture.Create<ExtendedWkbGeometry>(),
                        null);
                ((ISetProvenance)buildingWasMeasured).SetProvenance(Fixture.Create<Provenance>());

                buildingsWereMeasured.Add(buildingWasMeasured);
            }

            return buildingsWereMeasured.Select(x =>
                new Fact(new BuildingStreamId(new BuildingPersistentLocalId(x.BuildingPersistentLocalId)), x)).ToList();
        }
    }
}
