namespace BuildingRegistry.Tests.WhenImportingCrabTerrainObject
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Building.Commands.Crab;
    using Building.Events;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;
    using WhenImportingCrabBuildingStatus;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingIsRetired : AutofacBasedTest
    {
        private readonly Fixture _fixture;

        public GivenBuildingIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new NodaTimeCustomization());
            _fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithInfiniteLifetime());
            _fixture.Customize(new WithNoDeleteModification());
        }

        [Fact]
        public void WhenStatusWasUnderConstructionWithInfiniteLifetime()
        {
            var statusCommand = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.UnderConstruction);

            var command = _fixture.Create<ImportTerrainObjectFromCrab>();

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingBecameUnderConstruction>(),
                    statusCommand.ToLegacyEvent(),
                    _fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(buildingId,
                    new BuildingBecameUnderConstruction(buildingId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoStatusWithInfiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>();

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(buildingId,
                    new BuildingStatusWasRemoved(buildingId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenStatusWasPlannedWithInfiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>();
            var statusCommand = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.PermitRequested);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasPlanned>(),
                    statusCommand.ToLegacyEvent(),
                    _fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(buildingId,
                    new BuildingWasPlanned(buildingId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenStatusWasRealizedWithInfiniteLifetime()
        {
            var statusCommand = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.InUse);
            var command = _fixture.Create<ImportTerrainObjectFromCrab>();

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRealized>(),
                    statusCommand.ToLegacyEvent(),
                    _fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(buildingId,
                    new BuildingWasRealized(buildingId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenStatusWasRealizedWithInfiniteLifetimeAndCorrection()
        {
            var statusCommand = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.OutOfUse);

            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Correction);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRealized>(),
                    statusCommand.ToLegacyEvent(),
                    _fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(buildingId,
                    new BuildingWasCorrectedToRealized(buildingId),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithFiniteLifetime()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(_fixture.Create<LocalDateTime>(), _fixture.Create<LocalDateTime>()));

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRealized>(),
                    _fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(buildingId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenNoStatusWithInfiniteLifetimeAndCorrection()
        {
            var command = _fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Correction);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(buildingId,
                    new BuildingStatusWasCorrectedToRemoved(buildingId),
                    command.ToLegacyEvent()));
        }
    }
}
