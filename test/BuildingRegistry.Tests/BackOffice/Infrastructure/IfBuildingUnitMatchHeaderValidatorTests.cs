namespace BuildingRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AggregateTests.WhenPlanningBuildingUnit;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Building;
    using Building.Commands;
    using Building.Events;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Infrastructure;
    using Fixtures;
    using FluentAssertions;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
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
            await _backOfficeContext.SaveChangesAsync();

            var planBuilding = Fixture.Create<PlanBuilding>();
            DispatchArrangeCommand(planBuilding);

            var planBuildingUnit = Fixture.Create<PlanBuildingUnit>()
                .WithoutPosition()
                .WithPositionGeometryMethod(BuildingUnitPositionGeometryMethod.DerivedFromObject);
            DispatchArrangeCommand(planBuildingUnit);

            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 1, 1);

            var lastEvent = JsonConvert.DeserializeObject<BuildingUnitWasPlannedV2>(
                await stream.Messages.First().GetJsonData(),
                EventSerializerSettings);

            var validEtag = new ETag(ETagType.Strong, lastEvent!.GetHash());
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
            await _backOfficeContext.SaveChangesAsync();

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

        [Fact]
        public async Task WhenBuildingCantBeFoundThroughBuildingUnitPersistentLocalId_ThenThrowsAggregateIdIsNotFoundException()
        {
            // Arrange
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var eTag = new ETag(ETagType.Strong, "SomeHash");
            var sut = new IfMatchHeaderValidator(Container.Resolve<IBuildings>(), _backOfficeContext);

            // Act
            var act = async () => await sut.IsValidForBuildingUnit(eTag.ToString(), buildingUnitPersistentLocalId, CancellationToken.None);

            // Assert
            act.Should().ThrowAsync<AggregateIdIsNotFoundException>();
        }
    }
}
