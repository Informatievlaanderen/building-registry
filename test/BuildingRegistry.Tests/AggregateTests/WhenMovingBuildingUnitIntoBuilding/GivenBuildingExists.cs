namespace BuildingRegistry.Tests.AggregateTests.WhenMovingBuildingUnitIntoBuilding
{
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingGeometryMethod = Building.BuildingGeometryMethod;
    using BuildingUnitFunction = Building.BuildingUnitFunction;
    using BuildingUnitPositionGeometryMethod = Building.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = Building.BuildingUnitStatus;
    using ExtendedWkbGeometry = Building.ExtendedWkbGeometry;

    public class GivenBuildingExists : BuildingRegistryTest
    {
        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public void WithSourceBuildingRemoved_ThenThrowsBuildingIsRemovedException()
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>().WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasRemovedV2>().WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>().WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId))
                )
                .When(command)
                .Throws(new BuildingIsRemovedException(command.SourceBuildingPersistentLocalId)));
        }

        [Fact]
        public void WithBuildingUnitDoesNotExist_ThenThrowsBuildingUnitIsNotFoundException()
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>().WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>().WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId))
                )
                .When(command)
                .Throws(new BuildingUnitIsNotFoundException()));
        }

        [Fact]
        public void WithBuildingUnitRemoved_ThenThrowsBuildingUnitIsRemovedException()
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>().WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>().WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingUnitWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                            .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingUnitWasRemovedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                            .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId))
                )
                .When(command)
                .Throws(new BuildingUnitIsRemovedException(command.BuildingUnitPersistentLocalId)));
        }

        [Fact]
        public void WithBuildingUnitIsCommonUnit_ThenThrowsBuildingUnitHasInvalidFunctionException()
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();

            //TODO-rik customize CommonBuildingUnitWasAddedV2 voor geldige status

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>().WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>().WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<CommonBuildingUnitWasAddedV2>()
                            .WithBuildingUnitStatus(BuildingUnitStatus.Planned)
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                            .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId))
                )
                .When(command)
                .Throws(new BuildingUnitHasInvalidFunctionException()));
        }

        [Theory]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void WithInvalidDestinationBuildingStatus_ThenThrowsBuildingHasInvalidStatusException(string status)
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>().WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        new BuildingWasMigratedBuilder(Fixture)
                            .WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)
                            .WithBuildingStatus(status)
                            .Build()),
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingUnitWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                            .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                            .WithFunction(BuildingUnitFunction.Unknown))
                )
                .When(command)
                .Throws(new BuildingHasInvalidStatusException()));
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("UnderConstruction")]
        [InlineData("Realized")]
        public void WithValidBuildingStatus_ThenBuildingUnitMovedIntoBuilding(string buildingStatus)
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();

            var sourceBuildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()), BuildingGeometryMethod.MeasuredByGrb);
            var destinationBuildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()), BuildingGeometryMethod.MeasuredByGrb);

            var buildingUnitAddressPersistentLocalIds = Fixture.CreateMany<AddressPersistentLocalId>(5).ToList();

            var expectedBuildingUnitStatus = BuildingUnitStatus.Planned;
            var expectedGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
            var expectedGeometry = destinationBuildingGeometry.Center;

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        new BuildingWasMigratedBuilder(Fixture)
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                            .WithBuildingGeometry(sourceBuildingGeometry)
                            .WithBuildingUnit(
                                BuildingRegistry.Legacy.BuildingUnitStatus.Planned,
                                new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                                BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                                attachedAddresses: buildingUnitAddressPersistentLocalIds
                            )
                            .Build()),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        new BuildingWasMigratedBuilder(Fixture)
                            .WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)
                            .WithBuildingStatus(buildingStatus)
                            .WithBuildingGeometry(destinationBuildingGeometry)
                            .Build())
                )
                .When(command)
                .Then(
                    new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                    new BuildingUnitMovedIntoBuilding(
                        new BuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId),
                        new BuildingPersistentLocalId(command.SourceBuildingPersistentLocalId),
                        new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                        expectedBuildingUnitStatus,
                        expectedGeometryMethod,
                        expectedGeometry,
                        BuildingUnitFunction.Unknown,
                        false,
                        buildingUnitAddressPersistentLocalIds
                ))
            );
        }

        [Theory]
        [InlineData("Planned", "Planned")]
        [InlineData("UnderConstruction", "Planned")]
        [InlineData("Realized", "Realized")]
        public void WithBuildingUnitStatusRealized_ThenBuildingUnitMovedIntoBuilding(
            string buildingStatus,
            string expectedBuildingUnitStatus)
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();

            var sourceBuildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()), BuildingGeometryMethod.MeasuredByGrb);
            var destinationBuildingGeometry = new BuildingGeometry(new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()), BuildingGeometryMethod.MeasuredByGrb);

            var buildingUnitAddressPersistentLocalIds = Fixture.CreateMany<AddressPersistentLocalId>(5).ToList();

            var expectedGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
            var expectedGeometry = destinationBuildingGeometry.Center;

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        new BuildingWasMigratedBuilder(Fixture)
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                            .WithBuildingGeometry(sourceBuildingGeometry)
                            .WithBuildingUnit(
                                BuildingRegistry.Legacy.BuildingUnitStatus.Realized,
                                new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                                BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                                attachedAddresses: buildingUnitAddressPersistentLocalIds
                            )
                            .Build()),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        new BuildingWasMigratedBuilder(Fixture)
                            .WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)
                            .WithBuildingStatus(buildingStatus)
                            .WithBuildingGeometry(destinationBuildingGeometry)
                            .Build())
                )
                .When(command)
                .Then(
                    new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                    new BuildingUnitMovedIntoBuilding(
                        new BuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId),
                        new BuildingPersistentLocalId(command.SourceBuildingPersistentLocalId),
                        new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                        BuildingUnitStatus.Parse(expectedBuildingUnitStatus),
                        expectedGeometryMethod,
                        expectedGeometry,
                        BuildingUnitFunction.Unknown,
                        false,
                        buildingUnitAddressPersistentLocalIds
                ))
            );
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public void WithBuildingUnitStatusOtherThanRealized_ThenBuildingUnitMovedIntoBuilding(
            string buildingUnitStatus)
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();

            var sourceBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.ValidPolygon.AsBinary()),
                BuildingGeometryMethod.MeasuredByGrb);
            var destinationBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                BuildingGeometryMethod.MeasuredByGrb);

            var buildingUnitAddressPersistentLocalIds = Fixture.CreateMany<AddressPersistentLocalId>(5).ToList();

            var expectedGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
            var expectedGeometry = destinationBuildingGeometry.Center;

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        new BuildingWasMigratedBuilder(Fixture)
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                            .WithBuildingGeometry(sourceBuildingGeometry)
                            .WithBuildingUnit(
                                BuildingRegistry.Legacy.BuildingUnitStatus.Parse(buildingUnitStatus).Value,
                                new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                                BuildingRegistry.Legacy.BuildingUnitFunction.Unknown,
                                attachedAddresses: buildingUnitAddressPersistentLocalIds
                            )
                            .Build()),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        new BuildingWasMigratedBuilder(Fixture)
                            .WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)
                            .WithBuildingStatus(BuildingStatus.Realized)
                            .WithBuildingGeometry(destinationBuildingGeometry)
                            .Build())
                )
                .When(command)
                .Then(
                    new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                    new BuildingUnitMovedIntoBuilding(
                        new BuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId),
                        new BuildingPersistentLocalId(command.SourceBuildingPersistentLocalId),
                        new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                        BuildingUnitStatus.Parse(buildingUnitStatus),
                        expectedGeometryMethod,
                        expectedGeometry,
                        BuildingUnitFunction.Unknown,
                        false,
                        buildingUnitAddressPersistentLocalIds
                ))
            );
        }

        //TODO-rik unit tests scenarios:
        /*
        - als buildingunit.position.method == DerivedFromObject -> position moet center van destinationbuilding zijn + methode DerivedFromObject
        - als buildingunit.position.method == AppointedByAdministrator && geometry ligt buiten destinationbuilding geometry -> position moet center van destinationbuilding zijn + methode DerivedFromObject
        - als buildingunit.position.method == AppointedByAdministrator && geometry ligt binnen destinationbuilding geometry -> position behouden

        - happy flow: AddressPersistentLocalIds worden behouden*/

        [Fact]
        public void StateCheck()
        {
            //var plannedBuildingUnitPersistentLocalId = new PersistentLocalId(123);
            //var retiredBuildingUnitPersistentLocalId = new PersistentLocalId(456);
            //var realizedBuildingUnitPersistentLocalId = new PersistentLocalId(789);
            //var removedBuildingUnitPersistentLocalId = new PersistentLocalId(101);

            //var plannedBuildingUnit = new BuildingUnitBuilder(Fixture)
            //    .WithPersistentLocalId(plannedBuildingUnitPersistentLocalId)
            //    .WithFunction(BuildingUnitFunction.Unknown)
            //    .WithStatus(BuildingUnitStatus.Planned)
            //    .WithAddress(5)
            //    .Build();

            //var retiredBuildingUnit = new BuildingUnitBuilder(Fixture)
            //    .WithPersistentLocalId(retiredBuildingUnitPersistentLocalId)
            //    .WithFunction(BuildingUnitFunction.Unknown)
            //    .WithStatus(BuildingUnitStatus.Retired)
            //    .Build();

            //var realizedBuildingUnit = new BuildingUnitBuilder(Fixture)
            //    .WithPersistentLocalId(realizedBuildingUnitPersistentLocalId)
            //    .WithFunction(BuildingUnitFunction.Unknown)
            //    .WithStatus(BuildingUnitStatus.Realized)
            //    .Build();

            //var removedBuildingUnit = new BuildingUnitBuilder(Fixture)
            //    .WithPersistentLocalId(removedBuildingUnitPersistentLocalId)
            //    .WithStatus(BuildingUnitStatus.Planned)
            //    .WithIsRemoved()
            //    .Build();

            //var buildingWasMigrated = new BuildingWasMigratedBuilder(Fixture)
            //    .WithBuildingStatus(BuildingStatus.UnderConstruction)
            //    .WithBuildingUnit(plannedBuildingUnit)
            //    .WithBuildingUnit(retiredBuildingUnit)
            //    .WithBuildingUnit(realizedBuildingUnit)
            //    .WithBuildingUnit(removedBuildingUnit)
            //    .Build();

            //var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            //sut.Initialize(new List<object>
            //{
            //    buildingWasMigrated
            //});

            //// Act
            //sut.NotRealizeConstruction();

            //// Assert
            //sut.BuildingStatus.Should().Be(BuildingStatus.NotRealized);

            //var plannedUnit = sut.BuildingUnits
            //    .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(plannedBuildingUnitPersistentLocalId));

            //plannedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.NotRealized);
            //plannedUnit.AddressPersistentLocalIds.Should().BeEmpty();

            //var retiredUnit = sut.BuildingUnits
            //    .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(retiredBuildingUnitPersistentLocalId));

            //retiredUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Retired);

            //var realizedUnit = sut.BuildingUnits
            //    .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(realizedBuildingUnitPersistentLocalId));

            //realizedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Realized);

            //var removedUnit = sut.BuildingUnits
            //    .First(x => x.BuildingUnitPersistentLocalId == new BuildingUnitPersistentLocalId(removedBuildingUnitPersistentLocalId));

            //removedUnit.Status.Should().Be(BuildingRegistry.Building.BuildingUnitStatus.Planned);
        }
    }
}
