namespace BuildingRegistry.Tests.WhenImportingCrabTerrainObject
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using Building.Events;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;
    using WhenImportingCrabBuildingStatus;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingIsRetired : SnapshotBasedTest
    {
        public GivenBuildingIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithInfiniteLifetime());
            Fixture.Customize(new WithNoDeleteModification());
        }

        [Fact]
        public void WhenStatusWasUnderConstructionWithInfiniteLifetime()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var statusCommand = Fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.UnderConstruction);

            var command = Fixture.Create<ImportTerrainObjectFromCrab>();

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingBecameUnderConstruction>(),
                    statusCommand.ToLegacyEvent(),
                    Fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingBecameUnderConstruction(buildingId)),
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithStatusChronicle(statusCommand)
                        .WithStatus(BuildingStatus.UnderConstruction)
                        .WithLastModificationFromCrab(Modification.Update)
                        .Build(5, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WhenNoStatusWithInfiniteLifetime()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var command = Fixture.Create<ImportTerrainObjectFromCrab>();

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingStatusWasRemoved(buildingId)),
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithStatus(null)
                        .WithLastModificationFromCrab(Modification.Insert)
                        .Build(3, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WhenStatusWasPlannedWithInfiniteLifetime()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var command = Fixture.Create<ImportTerrainObjectFromCrab>();
            var statusCommand = Fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.PermitRequested);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasPlanned>(),
                    statusCommand.ToLegacyEvent(),
                    Fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingWasPlanned(buildingId)),
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithStatus(BuildingStatus.Planned)
                        .WithStatusChronicle(statusCommand)
                        .WithLastModificationFromCrab(Modification.Update)
                        .Build(5, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WhenStatusWasRealizedWithInfiniteLifetime()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var statusCommand = Fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.InUse);
            var command = Fixture.Create<ImportTerrainObjectFromCrab>();

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasRealized>(),
                    statusCommand.ToLegacyEvent(),
                    Fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingWasRealized(buildingId)),
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithStatus(BuildingStatus.Realized)
                        .WithStatusChronicle(statusCommand)
                        .WithLastModificationFromCrab(Modification.Update)
                        .Build(5, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WhenStatusWasRealizedWithInfiniteLifetimeAndCorrection()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var statusCommand = Fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.OutOfUse);

            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Correction);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasRealized>(),
                    statusCommand.ToLegacyEvent(),
                    Fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingWasCorrectedToRealized(buildingId)),
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithStatus(BuildingStatus.Realized)
                        .WithStatusChronicle(statusCommand)
                        .WithLastModificationFromCrab(Modification.Update)
                        .Build(5, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WithFiniteLifetime()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithLifetime(new CrabLifetime(Fixture.Create<LocalDateTime>(), Fixture.Create<LocalDateTime>()));

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasRealized>(),
                    Fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(new[]
                {
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithStatus(BuildingStatus.Retired)
                        .WithLastModificationFromCrab(Modification.Insert)
                        .Build(3, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WhenNoStatusWithInfiniteLifetimeAndCorrection()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Correction);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingStatusWasCorrectedToRemoved(buildingId)),
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder
                        .CreateDefaultSnapshot(buildingId)
                        .WithStatus(null)
                        .WithLastModificationFromCrab(Modification.Insert)
                        .Build(3, EventSerializerSettings))
                }));
        }
    }
}
