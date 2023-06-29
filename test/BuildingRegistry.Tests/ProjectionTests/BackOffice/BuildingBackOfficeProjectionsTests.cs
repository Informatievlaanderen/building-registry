namespace BuildingRegistry.Tests.ProjectionTests.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
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

            _fakeBackOfficeContext =
                new FakeBackOfficeContextFactory(dontDispose: true).CreateDbContext(Array.Empty<string>());
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
                    var result =
                        await _fakeBackOfficeContext.BuildingUnitBuildings.FindAsync(buildingUnitWasPlannedV2
                            .BuildingUnitPersistentLocalId);

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
                    var result =
                        await _fakeBackOfficeContext.BuildingUnitBuildings.FindAsync(buildingUnitWasPlannedV2
                            .BuildingUnitPersistentLocalId);

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
                    var result =
                        await _fakeBackOfficeContext.BuildingUnitBuildings.FindAsync(commonBuildingUnitWasAddedV2
                            .BuildingUnitPersistentLocalId);

                    result.Should().NotBeNull();
                    result!.BuildingPersistentLocalId.Should()
                        .Be(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
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
                    var result =
                        await _fakeBackOfficeContext.BuildingUnitBuildings.FindAsync(commonBuildingUnitWasAddedV2
                            .BuildingUnitPersistentLocalId);

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
                        buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId,
                        buildingUnitAddressWasAttachedV2.AddressPersistentLocalId);

                    result.Should().NotBeNull();
                    result!.BuildingPersistentLocalId.Should()
                        .Be(buildingUnitAddressWasAttachedV2.BuildingPersistentLocalId);
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
                        buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId,
                        buildingUnitAddressWasAttachedV2.AddressPersistentLocalId);

                    result.Should().NotBeNull();
                    result.Should().BeSameAs(expectedRelation);
                });
        }

        [Fact]
        public async Task GivenBuildingUnitAddressWasDetachedV2_ThenRelationIsRemoved()
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
                        buildingUnitAddressWasDetachedV2.BuildingUnitPersistentLocalId,
                        buildingUnitAddressWasDetachedV2.AddressPersistentLocalId);

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
                        buildingUnitAddressWasDetachedV2.BuildingUnitPersistentLocalId,
                        buildingUnitAddressWasDetachedV2.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenBuildingUnitAddressWasDetachedBecauseAddressWasRejected_ThenRelationIsRemoved()
        {
            var @event = _fixture.Create<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>();

            await _fakeBackOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                CancellationToken.None);

            await Sut
                .Given(@event)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        @event.BuildingUnitPersistentLocalId, @event.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenBuildingUnitAddressWasDetachedBecauseAddressWasRetired_ThenRelationIsRemoved()
        {
            var @event = _fixture.Create<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>();

            await _fakeBackOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                CancellationToken.None);

            await Sut
                .Given(@event)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        @event.BuildingUnitPersistentLocalId, @event.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenBuildingUnitAddressWasDetachedBecauseAddressWasRemoved_ThenRelationIsRemoved()
        {
            var @event = _fixture.Create<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>();

            await _fakeBackOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(@event.AddressPersistentLocalId),
                CancellationToken.None);

            await Sut
                .Given(@event)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        @event.BuildingUnitPersistentLocalId, @event.AddressPersistentLocalId);

                    result.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenBuildingUnitAddressWasReplacedBecauseAddressWasReaddressed_ThenRelationIsReplaced()
        {
            var @event = _fixture.Create<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>();

            await _fakeBackOfficeContext.AddBuildingUnitAddressRelation(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(@event.PreviousAddressPersistentLocalId));

            await Sut
                .Given(@event)
                .Then(async _ =>
                {
                    var result = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        @event.BuildingUnitPersistentLocalId, @event.NewAddressPersistentLocalId);

                    result.Should().NotBeNull();

                    var sourceRelation = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        @event.BuildingUnitPersistentLocalId, @event.PreviousAddressPersistentLocalId);
                    sourceRelation.Should().BeNull();
                });
        }

        [Fact]
        public async Task GivenBuildingUnitWasTransferred_ThenRelationIsRecoupled()
        {
            var newBuildingPersistentLocalId = _fixture.Create<BuildingPersistentLocalId>();
            var oldBuildingPersistentLocalId = _fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var addressPersistentLocalId1 = _fixture.Create<AddressPersistentLocalId>();
            var addressPersistentLocalId2 = _fixture.Create<AddressPersistentLocalId>();

            await _fakeBackOfficeContext.AddIdempotentBuildingUnitBuilding(oldBuildingPersistentLocalId, buildingUnitPersistentLocalId, CancellationToken.None);
            await _fakeBackOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                oldBuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                addressPersistentLocalId1, CancellationToken.None);
            await _fakeBackOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                oldBuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                addressPersistentLocalId2, CancellationToken.None);

            var buildingUnitWasTransferred = new BuildingUnitWasTransferred(
                newBuildingPersistentLocalId,
                BuildingUnit.Transfer(
                    _ => { },
                    newBuildingPersistentLocalId,
                    buildingUnitPersistentLocalId,
                    BuildingUnitFunction.Unknown,
                    BuildingUnitStatus.Realized,
                    new List<AddressPersistentLocalId> { addressPersistentLocalId1, addressPersistentLocalId2 },
                    new BuildingUnitPosition(new ExtendedWkbGeometry("".ToByteArray()), BuildingUnitPositionGeometryMethod.AppointedByAdministrator),
                    false),
                oldBuildingPersistentLocalId,
                new BuildingUnitPosition(new ExtendedWkbGeometry("".ToByteArray()), BuildingUnitPositionGeometryMethod.AppointedByAdministrator));

            await Sut
                .Given(buildingUnitWasTransferred)
                .Then(async _ =>
                {
                    var buildBuildingUnitRelation =
                        await _fakeBackOfficeContext.BuildingUnitBuildings.FindAsync((int)buildingUnitPersistentLocalId);

                    buildBuildingUnitRelation.Should().NotBeNull();
                    buildBuildingUnitRelation!.BuildingPersistentLocalId.Should().Be(newBuildingPersistentLocalId);

                    var buildingUnitAddressRelations =
                        await _fakeBackOfficeContext.FindAllBuildingUnitAddressRelations(buildingUnitPersistentLocalId, CancellationToken.None);

                    buildingUnitAddressRelations[0].BuildingPersistentLocalId.Should().Be(newBuildingPersistentLocalId);
                    buildingUnitAddressRelations[1].BuildingPersistentLocalId.Should().Be(newBuildingPersistentLocalId);
                });
        }
    }
}
