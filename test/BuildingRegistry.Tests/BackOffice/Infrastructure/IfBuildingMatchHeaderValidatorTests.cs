namespace BuildingRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building;
    using Building.Commands;
    using Building.Events;
    using BuildingRegistry.Api.BackOffice.Infrastructure;
    using Fixtures;
    using FluentAssertions;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;
    using Xunit.Abstractions;

    public class IfBuildingMatchHeaderValidatorTests : BuildingRegistryTest
    {
        private readonly FakeBackOfficeContext _backOfficeContext;

        public IfBuildingMatchHeaderValidatorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task WhenValidIfMatchHeader()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var planBuilding = Fixture.Create<PlanBuilding>();
            DispatchArrangeCommand(planBuilding);

            var placeBuildingUnderConstruction = Fixture.Create<PlaceBuildingUnderConstruction>();
            DispatchArrangeCommand(placeBuildingUnderConstruction);

            var stream = await Container.Resolve<IStreamStore>()
                .ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 1, 1);

            var lastEvent = JsonConvert.DeserializeObject<BuildingBecameUnderConstructionV2>(
                await stream.Messages.First().GetJsonData(),
                EventSerializerSettings);

            var validEtag = new ETag(ETagType.Strong, lastEvent!.GetHash());
            var sut = new IfMatchHeaderValidator(Container.Resolve<IBuildings>(), _backOfficeContext);

            // Act
            var result = await sut.IsValidForBuilding(validEtag.ToString(), buildingPersistentLocalId, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task WhenNotValidIfMatchHeader()
        {
            // Arrange
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            DispatchArrangeCommand(Fixture.Create<PlanBuilding>());

            var invalidEtag = new ETag(ETagType.Strong, "NotValidHash");
            var sut = new IfMatchHeaderValidator(Container.Resolve<IBuildings>(), _backOfficeContext);

            // Act
            var result = await sut.IsValidForBuilding(invalidEtag.ToString(), buildingPersistentLocalId, CancellationToken.None);

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
            var result = await sut.IsValidForBuilding(etag, Fixture.Create<BuildingPersistentLocalId>(), CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }
    }
}
