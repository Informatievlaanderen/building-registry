namespace BuildingRegistry.Tests.Legacy.WhenImportingCrabBuildingStatus
{
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using BuildingRegistry.Legacy;
    using BuildingRegistry.Legacy.Commands.Crab;
    using BuildingRegistry.Legacy.Crab;
    using BuildingRegistry.Legacy.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuilding : AutofacBasedTest
    {
        private readonly Fixture _fixture;

        public GivenBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithInfiniteLifetime());
        }

        [Fact]
        public void WhenStatusIsPermitRequested()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.PermitRequested);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasPlanned(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenStatusIsPermitGranted()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.BuildingPermitGranted);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasPlanned(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenStatusIsInUse()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.InUse);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasRealized(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenStatusIsUnderConstruction()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.UnderConstruction);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingBecameUnderConstruction(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenStatusIsOutOfUse()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.OutOfUse);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasRealized(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusPlannedWhenStatusIsTheSame()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.PermitRequested);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasPlanned>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusPermitRequestedWhenModificationIsDelete()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.PermitRequested)
                .WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusIsPlannedWhenStatusPermitRequestAndModificationIsDelete()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.PermitRequested)
                .WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasPlanned>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingStatusWasRemoved(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusIsPlannedWhenStatusInUseAndNewerLifetime()
        {
            var statusImported = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.PermitRequested);

            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithLifetime(new CrabLifetime(statusImported.Lifetime.BeginDateTime.Value.PlusDays(1), null))
                .WithStatus(CrabBuildingStatus.InUse);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasPlanned>(),
                    statusImported.ToLegacyEvent())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasRealized(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithStatusIsPlannedWhenStatusInUseAndOlderLifetime()
        {
            var statusImported = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.PermitRequested);

            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithLifetime(new CrabLifetime(statusImported.Lifetime.BeginDateTime.Value.PlusDays(-1), null))
                .WithStatus(CrabBuildingStatus.InUse);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasPlanned>(),
                    statusImported.ToLegacyEvent())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithRemovedStatusWhenStatusInUseAndSameLifetimeOfPreviouslyRemovedStatus()
        {
            var statusImported = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.PermitRequested);

            var statusImportedDelete = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.PermitRequested)
                .WithCrabModification(CrabModification.Delete)
                .WithLifetime(statusImported.Lifetime);

            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithLifetime(statusImported.Lifetime)
                .WithStatus(CrabBuildingStatus.InUse);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasPlanned>(),
                    statusImported.ToLegacyEvent(),
                    _fixture.Create<BuildingStatusWasRemoved>(),
                    statusImportedDelete.ToLegacyEvent())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasRealized(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WithRealizedStatusWhenDeletingOlderStatus()
        {
            var statusImported = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.PermitRequested);

            var deleteStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithBuildingStatusId(statusImported.BuildingStatusId)
                .WithStatus(CrabBuildingStatus.PermitRequested)
                .WithCrabModification(CrabModification.Delete);

            var realizedImported = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithLifetime(statusImported.Lifetime)
                .WithStatus(CrabBuildingStatus.InUse);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasPlanned>(),
                    statusImported.ToLegacyEvent(),
                    _fixture.Create<BuildingWasRealized>(),
                    realizedImported.ToLegacyEvent())
                .When(deleteStatus)
                .Then(_fixture.Create<BuildingId>(),
                    deleteStatus.ToLegacyEvent()));
        }
    }
}
