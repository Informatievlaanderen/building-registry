namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabTerrainObject
{
    using System.Collections.Generic;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuilding : AutofacBasedTest
    {
        private readonly Fixture _fixture;

        public GivenBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new NodaTimeCustomization());
            _fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());
            _fixture.Customize(new WithFixedBuildingId());
        }

        [Fact]
        public void WithModificationRemoved()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Delete);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                       _fixture.Create<BuildingWasRegistered>())
                .When(command)
                .Then(buildingId,
                    new BuildingWasRemoved(buildingId, new List<BuildingUnitId>()),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithFiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>();

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>())
                .When(command)
                .Then(buildingId,
                    new BuildingWasNotRealized(buildingId, new List<BuildingUnitId>(), new List<BuildingUnitId>()),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenRealizedWithFiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>();

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRealized>())
                .When(command)
                .Then(buildingId,
                    new BuildingWasRetired(buildingId, new List<BuildingUnitId>(), new List<BuildingUnitId>()),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithFiniteLifetimeAndCorrection()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Correction);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>())
                .When(command)
                .Then(buildingId,
                    new BuildingWasCorrectedToNotRealized(buildingId, new List<BuildingUnitId>(), new List<BuildingUnitId>()),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenRealizedWithFiniteLifetimeAndCorrection()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Correction);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRealized>())
                .When(command)
                .Then(buildingId,
                    new BuildingWasCorrectedToRetired(buildingId, new List<BuildingUnitId>(), new List<BuildingUnitId>()),
                    command.ToLegacyEvent()));
        }
    }
}
