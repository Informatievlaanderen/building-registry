namespace BuildingRegistry.Tests.WhenImportingCrabBuildingStatus
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using Building.Events;
    using ValueObjects;
    using ValueObjects.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingIsRetired : SnapshotBasedTest
    {
        public GivenBuildingIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithNoDeleteModification());
        }

        [Theory]
        [InlineData(CrabBuildingStatus.InUse)]
        [InlineData(CrabBuildingStatus.OutOfUse)]
        public void WithMappedToRealizedThenNoStatusChangeIsApplied(CrabBuildingStatus status)
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var buildingId = Fixture.Create<BuildingId>();

            var importStatus = Fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(status);
            var buildingWasRetired = Fixture.Create<BuildingWasRetired>()
                .WithNoRetiredUnits();
            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingWasRetired)
                .When(importStatus)
                .Then(new[]
                {
                    new Fact(buildingId, importStatus.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithStatusChronicle(importStatus)
                            .WithStatus(BuildingStatus.Retired)
                            .WithLastModificationFromCrab(Modification.Insert)
                            .Build(2, EventSerializerSettings))
                }));
        }

        [Theory]
        [InlineData(CrabBuildingStatus.UnderConstruction)]
        [InlineData(CrabBuildingStatus.PermitRequested)]
        [InlineData(CrabBuildingStatus.BuildingPermitGranted)]
        public void WithMappedToNotRealizedThenNoStatusChangeIsApplied(CrabBuildingStatus status)
        {
            var importStatus = Fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(status);

            Assert(new Scenario()
                .Given(Fixture.Create<BuildingId>(),
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(importStatus)
                .Then(Fixture.Create<BuildingId>(),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabBuildingStatus.InUse)]
        [InlineData(CrabBuildingStatus.OutOfUse)]
        public void WithMappedToRealizedWhenNotRealized(CrabBuildingStatus status)
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var buildingId = Fixture.Create<BuildingId>();

            var importStatus = Fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(status);

            var buildingWasRetired = Fixture.Create<BuildingWasRetired>()
                .WithNoRetiredUnits();

            Assert(new Scenario()
                .Given(Fixture.Create<BuildingId>(),
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(importStatus)
                .Then(new[]
                {
                    new Fact(buildingId, buildingWasRetired),
                    new Fact(buildingId, importStatus.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithStatusChronicle(importStatus)
                            .WithStatus(BuildingStatus.Retired)
                            .WithLastModificationFromCrab(Modification.Insert)
                            .Build(3, EventSerializerSettings))
                }));
        }


        [Theory]
        [InlineData(CrabBuildingStatus.UnderConstruction)]
        [InlineData(CrabBuildingStatus.PermitRequested)]
        [InlineData(CrabBuildingStatus.BuildingPermitGranted)]
        public void WithMappedToNotRealizedWhenRetired(CrabBuildingStatus status)
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var buildingId = Fixture.Create<BuildingId>();

            var importStatus = Fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(status);

            var buildingWasNotRealized = Fixture.Create<BuildingWasNotRealized>()
                .WithNoRetiredUnits();

            Assert(new Scenario()
                .Given(Fixture.Create<BuildingId>(),
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(importStatus)
                .Then(new[]
                {
                    new Fact(buildingId, buildingWasNotRealized),
                    new Fact(buildingId, importStatus.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithStatusChronicle(importStatus)
                            .WithStatus(BuildingStatus.NotRealized)
                            .WithLastModificationFromCrab(Modification.Insert)
                            .Build(3, EventSerializerSettings))
                }));
        }

        ////TBD: Ruben question
        //[Fact]
        //public void WithDeletedWhenNotRealized()
        //{
        //    var importStatus = Fixture.Create<ImportBuildingStatusFromCrab>()
        //        .WithCrabModification(CrabModification.Delete);

        //    Assert(new Scenario()
        //        .Given(Fixture.Create<BuildingId>(),
        //            Fixture.Create<BuildingWasRegistered>(),
        //            Fixture.Create<BuildingWasNotRealized>()
        //                .WithNoRetiredUnits())
        //        .When(importStatus)
        //        .Then(Fixture.Create<BuildingId>(),
        //            importStatus.ToLegacyEvent()));
        //}

        ////TBD: Ruben question
        //[Fact]
        //public void WithDeletedWhenRetired()
        //{
        //    var importStatus = Fixture.Create<ImportBuildingStatusFromCrab>()
        //        .WithCrabModification(CrabModification.Delete);

        //    Assert(new Scenario()
        //        .Given(Fixture.Create<BuildingId>(),
        //            Fixture.Create<BuildingWasRegistered>(),
        //            Fixture.Create<BuildingWasRetired>()
        //                .WithNoRetiredUnits())
        //        .When(importStatus)
        //        .Then(Fixture.Create<BuildingId>(),
        //            new BuildingVersionWasIncreased(Fixture.Create<BuildingId>(), new Version(1)),
        //            new BuildingWasNotRealized(Fixture.Create<BuildingId>(), new List<BuildingUnitId>(), new List<BuildingUnitId>()),
        //            importStatus.ToLegacyEvent()));
        //}
    }
}
