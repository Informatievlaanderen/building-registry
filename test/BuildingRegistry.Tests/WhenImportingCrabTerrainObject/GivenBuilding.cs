namespace BuildingRegistry.Tests.WhenImportingCrabTerrainObject
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Events;
    using Building.Commands.Crab;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuilding : SnapshotBasedTest
    {
        public GivenBuilding(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
        }

        [Fact]
        public void WithModificationRemoved()
        {
            Fixture.Customize(new WithSnapshotInterval(1));

            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Delete);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>())
                .When(command)
                .Then(new[]
                    {
                       new Fact(buildingId, new BuildingWasRemoved(buildingId, new List<BuildingUnitId>())),
                       new Fact(buildingId, command.ToLegacyEvent()),
                       new Fact(GetSnapshotIdentifier(buildingId),
                           SnapshotBuilder
                               .CreateDefaultSnapshot(buildingId)
                               .WithIsRemoved(true)
                               .WithLastModificationFromCrab(Modification.Delete)
                               .Build(2, EventSerializerSettings))
                    }));
        }

        [Fact]
        public void WithFiniteLifetime()
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>();

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>())
                .When(command)
                .Then(buildingId,
                    new BuildingWasNotRealized(buildingId, new List<BuildingUnitId>(), new List<BuildingUnitId>()),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenRealizedWithFiniteLifetime()
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>();

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasRealized>())
                .When(command)
                .Then(buildingId,
                    new BuildingWasRetired(buildingId, new List<BuildingUnitId>(), new List<BuildingUnitId>()),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithFiniteLifetimeAndCorrection()
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Correction);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>())
                .When(command)
                .Then(buildingId,
                    new BuildingWasCorrectedToNotRealized(buildingId, new List<BuildingUnitId>(), new List<BuildingUnitId>()),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenRealizedWithFiniteLifetimeAndCorrection()
        {
            var command = Fixture.Create<ImportTerrainObjectFromCrab>()
                .WithModification(CrabModification.Correction);

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasRealized>())
                .When(command)
                .Then(buildingId,
                    new BuildingWasCorrectedToRetired(buildingId, new List<BuildingUnitId>(), new List<BuildingUnitId>()),
                    command.ToLegacyEvent()));
        }
    }
}
