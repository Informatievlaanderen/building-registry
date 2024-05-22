namespace BuildingRegistry.Tests.ProjectionTests.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Building;
    using Building.Events;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using FluentAssertions;
    using Xunit;

    public partial class BuildingBackOfficeProjectionsTests
    {
        [Fact]
        public async Task GivenOnlyPreviousBuildingUnitAddressRelationExistsWithCountOne_ThenRelationIsReplaced()
        {
            var @event = _fixture.Create<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>();

            await _fakeBackOfficeContext.AddBuildingUnitAddressRelation(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(@event.PreviousAddressPersistentLocalId));

            await Sut
                .Given(BuildEnvelope(@event))
                .Then(async _ =>
                {
                    var previousAddressRelation = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        @event.BuildingUnitPersistentLocalId, @event.PreviousAddressPersistentLocalId);
                    previousAddressRelation.Should().BeNull();

                    var newAddressRelation = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        @event.BuildingUnitPersistentLocalId, @event.NewAddressPersistentLocalId);
                    newAddressRelation.Should().NotBeNull();
                    newAddressRelation!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task GivenOnlyPreviousBuildingUnitAddressRelationExistsWithCountTwo_ThenCountIsDecrementedByOne()
        {
            var @event = _fixture.Create<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>();

            var relation = new BuildingUnitAddressRelation(
                @event.BuildingPersistentLocalId,
                @event.BuildingUnitPersistentLocalId,
                @event.PreviousAddressPersistentLocalId)
            {
                Count = 2
            };
            await _fakeBackOfficeContext.BuildingUnitAddressRelation.AddAsync(relation, CancellationToken.None);
            await _fakeBackOfficeContext.SaveChangesAsync(CancellationToken.None);

            await Sut
                .Given(BuildEnvelope(@event))
                .Then(async _ =>
                {
                    var previousAddressRelation = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        @event.BuildingUnitPersistentLocalId, @event.PreviousAddressPersistentLocalId);
                    previousAddressRelation.Should().NotBeNull();
                    previousAddressRelation!.Count.Should().Be(1);

                    var newAddressRelation = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        @event.BuildingUnitPersistentLocalId, @event.NewAddressPersistentLocalId);
                    newAddressRelation.Should().NotBeNull();
                    newAddressRelation!.Count.Should().Be(1);
                });
        }

        [Fact]
        public async Task GivenNewBuildingUnitAddressRelationExists_ThenCountIsIncrementedByOne()
        {
            var @event = _fixture.Create<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>();

            await _fakeBackOfficeContext.AddBuildingUnitAddressRelation(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(@event.PreviousAddressPersistentLocalId));
            await _fakeBackOfficeContext.AddBuildingUnitAddressRelation(
                new BuildingPersistentLocalId(@event.BuildingPersistentLocalId),
                new BuildingUnitPersistentLocalId(@event.BuildingUnitPersistentLocalId),
                new AddressPersistentLocalId(@event.NewAddressPersistentLocalId));

            await Sut
                .Given(BuildEnvelope(@event))
                .Then(async _ =>
                {
                    var previousAddressRelation = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        @event.BuildingUnitPersistentLocalId, @event.PreviousAddressPersistentLocalId);
                    previousAddressRelation.Should().BeNull();

                    var newAddressRelation = await _fakeBackOfficeContext.BuildingUnitAddressRelation.FindAsync(
                        @event.BuildingUnitPersistentLocalId, @event.NewAddressPersistentLocalId);
                    newAddressRelation.Should().NotBeNull();
                    newAddressRelation!.Count.Should().Be(2);
                });
        }
    }
}
