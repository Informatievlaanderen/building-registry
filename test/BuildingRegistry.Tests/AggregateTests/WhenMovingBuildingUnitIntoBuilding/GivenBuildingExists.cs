namespace BuildingRegistry.Tests.AggregateTests.WhenMovingBuildingUnitIntoBuilding
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Building;
    using Building.Commands;
    using Building.Events;
    using Building.Exceptions;
    using Extensions;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;
    using BuildingGeometry = Building.BuildingGeometry;
    using BuildingGeometryMethod = Building.BuildingGeometryMethod;
    using BuildingUnitFunction = Building.BuildingUnitFunction;
    using BuildingUnitPositionGeometryMethod = Building.BuildingUnitPositionGeometryMethod;
    using BuildingUnitStatus = Building.BuildingUnitStatus;
    using BuildingUnitWasPlannedV2 = Building.Events.BuildingUnitWasPlannedV2;
    using BuildingUnitWasRemovedV2 = Building.Events.BuildingUnitWasRemovedV2;
    using BuildingWasPlannedV2 = Building.Events.BuildingWasPlannedV2;
    using BuildingWasRemovedV2 = Building.Events.BuildingWasRemovedV2;
    using CommonBuildingUnitWasAddedV2 = Building.Events.CommonBuildingUnitWasAddedV2;
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

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>().WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>().WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)),
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<CommonBuildingUnitWasAddedV2>()
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
                    new BuildingUnitWasMovedIntoBuilding(
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
                    new BuildingUnitWasMovedIntoBuilding(
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
                    new BuildingUnitWasMovedIntoBuilding(
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

        [Fact]
        public void WithBuildingUnitPositionDerivedFromObject_ThenBuildingUnitMovedIntoBuilding()
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();
            var deviation = Fixture.Create<bool>();

            var destinationBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                BuildingGeometryMethod.MeasuredByGrb);

            var expectedGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
            var expectedGeometry = destinationBuildingGeometry.Center;

            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                        ),
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingUnitWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                            .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                            .WithGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject)
                            .WithDeviation(deviation)
                        ),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)
                            .WithGeometry(destinationBuildingGeometry.Geometry)
                        )
                )
                .When(command)
                .Then(
                    new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                    new BuildingUnitWasMovedIntoBuilding(
                        new BuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId),
                        new BuildingPersistentLocalId(command.SourceBuildingPersistentLocalId),
                        new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                        BuildingUnitStatus.Planned,
                        expectedGeometryMethod,
                        expectedGeometry,
                        BuildingUnitFunction.Unknown,
                        deviation,
                        new List<AddressPersistentLocalId>()
                ))
            );
        }

        [Fact]
        public void WithBuildingUnitPositionAppointedByAdministratorGeometryOutsideOfDestination_ThenBuildingUnitMovedIntoBuildingWithDerivedGeometryMethod()
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();
            var deviation = Fixture.Create<bool>();

            var buildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(GeometryHelper.PointNotInPolygon.AsBinary()),
                BuildingUnitPositionGeometryMethod.AppointedByAdministrator);

            var destinationBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                BuildingGeometryMethod.MeasuredByGrb);

            var expectedGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject;
            var expectedGeometry = destinationBuildingGeometry.Center;
            
            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                        ),
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingUnitWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                            .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                            .WithPosition(buildingUnitPosition)
                            .WithDeviation(deviation)
                        ),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)
                            .WithGeometry(destinationBuildingGeometry.Geometry)
                        )
                )
                .When(command)
                .Then(
                    new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                    new BuildingUnitWasMovedIntoBuilding(
                        new BuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId),
                        new BuildingPersistentLocalId(command.SourceBuildingPersistentLocalId),
                        new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                        BuildingUnitStatus.Planned,
                        expectedGeometryMethod,
                        expectedGeometry,
                        BuildingUnitFunction.Unknown,
                        deviation,
                        new List<AddressPersistentLocalId>()
                ))
            );
        }

        [Fact]
        public void WithBuildingUnitPositionAppointedByAdministratorGeometryInsideOfDestination_ThenBuildingUnitMovedIntoBuilding()
        {
            var command = Fixture.Create<MoveBuildingUnitIntoBuilding>();
            var deviation = Fixture.Create<bool>();

            var buildingUnitPosition = new BuildingUnitPosition(
                new ExtendedWkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary()),
                BuildingUnitPositionGeometryMethod.AppointedByAdministrator);

            var destinationBuildingGeometry = new BuildingGeometry(
                new ExtendedWkbGeometry(GeometryHelper.SecondValidPolygon.AsBinary()),
                BuildingGeometryMethod.MeasuredByGrb);

            var expectedGeometryMethod = BuildingUnitPositionGeometryMethod.AppointedByAdministrator;
            var expectedGeometry = buildingUnitPosition.Geometry;
            
            Assert(new Scenario()
                .Given(
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                        ),
                    new Fact(new BuildingStreamId(command.SourceBuildingPersistentLocalId),
                        Fixture.Create<BuildingUnitWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.SourceBuildingPersistentLocalId)
                            .WithBuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId)
                            .WithPosition(buildingUnitPosition)
                            .WithDeviation(deviation)
                        ),
                    new Fact(new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                        Fixture.Create<BuildingWasPlannedV2>()
                            .WithBuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId)
                            .WithGeometry(destinationBuildingGeometry.Geometry)
                        )
                )
                .When(command)
                .Then(
                    new BuildingStreamId(command.DestinationBuildingPersistentLocalId),
                    new BuildingUnitWasMovedIntoBuilding(
                        new BuildingPersistentLocalId(command.DestinationBuildingPersistentLocalId),
                        new BuildingPersistentLocalId(command.SourceBuildingPersistentLocalId),
                        new BuildingUnitPersistentLocalId(command.BuildingUnitPersistentLocalId),
                        BuildingUnitStatus.Planned,
                        expectedGeometryMethod,
                        expectedGeometry,
                        BuildingUnitFunction.Unknown,
                        deviation,
                        new List<AddressPersistentLocalId>()
                ))
            );
        }

        [Fact]
        public void StateCheck()
        {
            var buildingUnitWasMovedIntoBuilding = Fixture.Create<BuildingUnitWasMovedIntoBuilding>();
            var buildingWasPlanned = Fixture.Create<BuildingWasPlannedV2>()
                .WithBuildingPersistentLocalId(buildingUnitWasMovedIntoBuilding.BuildingPersistentLocalId);
            
            // Act
            var sut = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object>
            {
                buildingWasPlanned,
                buildingUnitWasMovedIntoBuilding
            });

            // Assert
            sut.BuildingUnits.Should().ContainSingle();
            var buildingUnit = sut.BuildingUnits.Single();
            buildingUnit.BuildingUnitPersistentLocalId.Should().Be(
                new BuildingUnitPersistentLocalId(buildingUnitWasMovedIntoBuilding.BuildingUnitPersistentLocalId));
            buildingUnit.BuildingUnitPosition.Should().Be(
                new BuildingUnitPosition(new ExtendedWkbGeometry(buildingUnitWasMovedIntoBuilding.ExtendedWkbGeometry),
                    BuildingUnitPositionGeometryMethod.Parse(buildingUnitWasMovedIntoBuilding.GeometryMethod)));
            buildingUnit.Function.Should().Be(BuildingUnitFunction.Parse(buildingUnitWasMovedIntoBuilding.Function));
            buildingUnit.HasDeviation.Should().Be(buildingUnitWasMovedIntoBuilding.HasDeviation);
            buildingUnit.Status.Should().Be(BuildingUnitStatus.Parse(buildingUnitWasMovedIntoBuilding.BuildingUnitStatus));
            buildingUnit.IsRemoved.Should().BeFalse();
            buildingUnit.AddressPersistentLocalIds.Should().BeEquivalentTo(
                buildingUnitWasMovedIntoBuilding.AddressPersistentLocalIds.Select(x => new AddressPersistentLocalId(x)).ToList());
        }
    }
}
