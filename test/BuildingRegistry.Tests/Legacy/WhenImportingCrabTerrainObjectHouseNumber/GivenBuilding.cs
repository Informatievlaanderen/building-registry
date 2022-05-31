namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabTerrainObjectHouseNumber
{
    using System;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuilding : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new NodaTimeCustomization());
            _fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithInfiniteLifetime());
        }

        [Fact]
        public void ThenBuildingUnitWasAdded()
        {
            _fixture.Customize(new WithNoDeleteModification());

            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();
            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitKey = BuildingUnitKey.Create(command.TerrainObjectId, command.TerrainObjectHouseNumberId);
            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitWasAdded(buildingId, BuildingUnitId.Create(buildingUnitKey, 1), buildingUnitKey, AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp)),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenSameBuildingUnit()
        {
            _fixture.Customize(new WithNoDeleteModification());

            var command = _fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();
            var buildingId = _fixture.Create<BuildingId>();

            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, new BuildingUnitId(Guid.NewGuid()), BuildingUnitKey.Create(command.TerrainObjectId, command.TerrainObjectHouseNumberId), AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp));
            ((ISetProvenance)buildingUnitWasAdded).SetProvenance(_fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded)
                .When(command)
                .Then(buildingId,
                    command.ToLegacyEvent()));
        }
    }
}
