namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabTerrainObjectHouseNumber
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingIsRetired : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new NodaTimeCustomization());
            _fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        // Check if adding building unit to retired building, building unit is retired
        [Fact]
        public void WithInfiniteLifetimeThenRetireNewlyAddedBuildingUnit()
        {
            _fixture.Customize(new WithInfiniteLifetime());
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitWasAddedToRetiredBuilding(buildingId, _fixture.Create<BuildingUnitId>(), _fixture.Create<BuildingUnitKey>(), AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp)),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithFiniteLifetimeThenRetireNewlyAddedBuildingUnit()
        {
            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitWasAddedToRetiredBuilding(buildingId, _fixture.Create<BuildingUnitId>(), _fixture.Create<BuildingUnitKey>(), AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp)),
                    command.ToLegacyEvent()));
        }
    }
}
