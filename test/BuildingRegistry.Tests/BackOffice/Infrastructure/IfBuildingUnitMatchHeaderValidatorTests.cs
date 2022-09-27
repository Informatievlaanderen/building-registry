namespace BuildingRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AggregateTests.WhenPlanningBuildingUnit;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Infrastructure;
    using Fixtures;
    using FluentAssertions;
    using Xunit;
    using Xunit.Abstractions;

    public class IfBuildingUnitMatchHeaderValidatorTests : BuildingRegistryTest
    {
        private readonly FakeBackOfficeContext _backOfficeContext;

        public IfBuildingUnitMatchHeaderValidatorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task WhenValidIfMatchHeader()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            _backOfficeContext.BuildingUnitBuildings.Add(new BuildingUnitBuilding(
                buildingUnitPersistentLocalId,
                buildingPersistentLocalId));
            _backOfficeContext.SaveChanges();

            var planBuilding = Fixture.Create<PlanBuilding>();
            var buildingGeometry = new BuildingGeometry(planBuilding.Geometry, BuildingGeometryMethod.Outlined);
            DispatchArrangeCommand(planBuilding);

            var planBuildingUnit = Fixture.Create<PlanBuildingUnit>()
                .WithoutPosition()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);
            DispatchArrangeCommand(planBuildingUnit);

            var lastEvent = new BuildingUnitWasPlannedV2(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                planBuildingUnit.PositionGeometryMethod,
                buildingGeometry.Center,
                planBuildingUnit.Function,
                planBuildingUnit.HasDeviation);
            ((ISetProvenance)lastEvent).SetProvenance(planBuildingUnit.Provenance);

            var validEtag = new ETag(ETagType.Strong, lastEvent.GetHash());
            var sut = new IfMatchHeaderValidator(Container.Resolve<IBuildings>(), _backOfficeContext);

            // Act
           var result = await sut.IsValidForBuildingUnit(validEtag.ToString(), buildingUnitPersistentLocalId, CancellationToken.None);

           // Assert
           result.Should().BeTrue();
        }

        [Fact]
        public async Task WhenNotValidIfMatchHeader()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            _backOfficeContext.BuildingUnitBuildings.Add(new BuildingUnitBuilding(
                buildingUnitPersistentLocalId,
                buildingPersistentLocalId));
            _backOfficeContext.SaveChanges();

            DispatchArrangeCommand(Fixture.Create<PlanBuilding>());
            DispatchArrangeCommand(Fixture.Create<PlanBuildingUnit>()
                .WithoutPosition()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject));

            var invalidEtag = new ETag(ETagType.Strong, "NotValidHash");
            var sut = new IfMatchHeaderValidator(Container.Resolve<IBuildings>(), _backOfficeContext);

            // Act
            var result = await sut.IsValidForBuildingUnit(invalidEtag.ToString(), buildingUnitPersistentLocalId, CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }


        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task WhenNoIfMatchHeader(string? etag)
        {
            // Arrange
            var sut = new IfMatchHeaderValidator(Container.Resolve<IBuildings>(), _backOfficeContext);

            // Act
            var result = await sut.IsValidForBuildingUnit(etag, Fixture.Create<BuildingUnitPersistentLocalId>(), CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }
    }
}
