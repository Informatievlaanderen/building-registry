namespace BuildingRegistry.Tests.ProjectionTests.BackOffice
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Building;
    using Building.Events;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using Tests.BackOffice;
    using Tests.Legacy.Autofixture;
    using Xunit;

    public class BuildingBackOfficeProjectionsTests : BuildingBackOfficeProjectionsTest
    {
        private readonly Fixture _fixture;
        private readonly FakeBackOfficeContext _fakeBackOfficeContext;

        public BuildingBackOfficeProjectionsTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithBuildingUnitPositionGeometryMethod());
            _fixture.Customize(new WithBuildingUnitFunction());
            _fixture.Customize(new WithBuildingUnitStatus());

            _fakeBackOfficeContext = new FakeBackOfficeContextFactory(dontDispose: true).CreateDbContext(Array.Empty<string>());
            BackOfficeContextMock
                .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fakeBackOfficeContext);
        }

        [Fact]
        public async Task GivenBuildingUnitWasPlannedV2_ThenRelationIsAdded()
        {
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            await Sut
                .Given(buildingUnitWasPlannedV2)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitBuildings.FindAsync(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);

                    result.Should().NotBeNull();
                    result!.BuildingPersistentLocalId.Should().Be(buildingUnitWasPlannedV2.BuildingPersistentLocalId);
                });
        }

        [Fact]
        public async Task GivenBuildingUnitWasPlannedV2AndRelationPresent_ThenNothing()
        {
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            var expectedRelation = await _fakeBackOfficeContext.AddIdempotentBuildingUnitBuilding(
                buildingUnitWasPlannedV2.BuildingPersistentLocalId,
                buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId,
                CancellationToken.None);

            await Sut
                .Given(buildingUnitWasPlannedV2)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitBuildings.FindAsync(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);

                    result.Should().NotBeNull();
                    result.Should().BeSameAs(expectedRelation);
                });
        }

        [Fact]
        public async Task GivenCommonBuildingUnitWasAddedV2_ThenRelationIsAdded()
        {
            var commonBuildingUnitWasAddedV2 = _fixture.Create<CommonBuildingUnitWasAddedV2>();

            await Sut
                .Given(commonBuildingUnitWasAddedV2)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitBuildings.FindAsync(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId);

                    result.Should().NotBeNull();
                    result!.BuildingPersistentLocalId.Should().Be(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
                });
        }

        [Fact]
        public async Task GivenCommonBuildingUnitWasAddedV2AndRelationPresent_ThenNothing()
        {
            var commonBuildingUnitWasAddedV2 = _fixture.Create<CommonBuildingUnitWasAddedV2>();

            var expectedRelation = await _fakeBackOfficeContext.AddIdempotentBuildingUnitBuilding(
                commonBuildingUnitWasAddedV2.BuildingPersistentLocalId,
                commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId,
                CancellationToken.None);

            await Sut
                .Given(commonBuildingUnitWasAddedV2)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitBuildings.FindAsync(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId);

                    result.Should().NotBeNull();
                    result.Should().BeSameAs(expectedRelation);
                });
        }

        [Fact]
        public async Task GivenBuildingUnitAddressWasAttachedV2_ThenRelationIsAdded()
        {
            var buildingUnitAddressWasAttachedV2 = _fixture.Create<BuildingUnitAddressWasAttachedV2>();

            await Sut
                .Given(buildingUnitAddressWasAttachedV2)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId, buildingUnitAddressWasAttachedV2.AddressPersistentLocalId);

                    result.Should().NotBeNull();
                    result!.BuildingPersistentLocalId.Should().Be(buildingUnitAddressWasAttachedV2.BuildingPersistentLocalId);
                });
        }

        [Fact]
        public async Task GivenBuildingUnitAddressWasAttachedV2AndRelationPresent_ThenNothing()
        {
            var buildingUnitAddressWasAttachedV2 = _fixture.Create<BuildingUnitAddressWasAttachedV2>();

            var expectedRelation = await _fakeBackOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                new BuildingPersistentLocalId(buildingUnitAddressWasAttachedV2.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId),
                CancellationToken.None);

            await Sut
                .Given(buildingUnitAddressWasAttachedV2)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId, buildingUnitAddressWasAttachedV2.AddressPersistentLocalId);

                    result.Should().NotBeNull();
                    result.Should().BeSameAs(expectedRelation);
                });
        }

        [Fact]
        public async Task GivenBuildingUnitAddressWasDetachedV2_ThenRelationIsRemovedAdded()
        {
            var buildingUnitAddressWasDetachedV2 = _fixture.Create<BuildingUnitAddressWasDetachedV2>();

            await _fakeBackOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                new BuildingPersistentLocalId(buildingUnitAddressWasDetachedV2.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(buildingUnitAddressWasDetachedV2.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(buildingUnitAddressWasDetachedV2.AddressPersistentLocalId),
                CancellationToken.None);

            await Sut
                .Given(buildingUnitAddressWasDetachedV2)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        buildingUnitAddressWasDetachedV2.BuildingUnitPersistentLocalId, buildingUnitAddressWasDetachedV2.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenBuildingUnitAddressWasDetachedV2AndRelationDoesntExist_ThenNothing()
        {
            var buildingUnitAddressWasDetachedV2 = _fixture.Create<BuildingUnitAddressWasDetachedV2>();

            await Sut
                .Given(buildingUnitAddressWasDetachedV2)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        buildingUnitAddressWasDetachedV2.BuildingUnitPersistentLocalId, buildingUnitAddressWasDetachedV2.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }
    }
}
