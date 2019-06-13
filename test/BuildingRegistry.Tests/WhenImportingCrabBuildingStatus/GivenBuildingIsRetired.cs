namespace BuildingRegistry.Tests.WhenImportingCrabBuildingStatus
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Building.Commands.Crab;
    using Building.Events;
    using ValueObjects;
    using ValueObjects.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingIsRetired : AutofacBasedTest
    {
        private readonly Fixture _fixture;

        public GivenBuildingIsRetired(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithNoDeleteModification());
        }

        [Theory]
        [InlineData(CrabBuildingStatus.InUse)]
        [InlineData(CrabBuildingStatus.OutOfUse)]
        public void WithMappedToRealizedThenNoStatusChangeIsApplied(CrabBuildingStatus status)
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(status);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabBuildingStatus.UnderConstruction)]
        [InlineData(CrabBuildingStatus.PermitRequested)]
        [InlineData(CrabBuildingStatus.BuildingPermitGranted)]
        public void WithMappedToNotRealizedThenNoStatusChangeIsApplied(CrabBuildingStatus status)
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(status);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    importStatus.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabBuildingStatus.InUse)]
        [InlineData(CrabBuildingStatus.OutOfUse)]
        public void WithMappedToRealizedWhenNotRealized(CrabBuildingStatus status)
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(status);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasRetired(_fixture.Create<BuildingId>(), new List<BuildingUnitId>(), new List<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }


        [Theory]
        [InlineData(CrabBuildingStatus.UnderConstruction)]
        [InlineData(CrabBuildingStatus.PermitRequested)]
        [InlineData(CrabBuildingStatus.BuildingPermitGranted)]
        public void WithMappedToNotRealizedWhenRetired(CrabBuildingStatus status)
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(status);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasRetired>()
                        .WithNoRetiredUnits())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasNotRealized(_fixture.Create<BuildingId>(), new List<BuildingUnitId>(), new List<BuildingUnitId>()),
                    importStatus.ToLegacyEvent()));
        }

        ////TBD: Ruben question
        //[Fact]
        //public void WithDeletedWhenNotRealized()
        //{
        //    var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
        //        .WithCrabModification(CrabModification.Delete);

        //    Assert(new Scenario()
        //        .Given(_fixture.Create<BuildingId>(),
        //            _fixture.Create<BuildingWasRegistered>(),
        //            _fixture.Create<BuildingWasNotRealized>()
        //                .WithNoRetiredUnits())
        //        .When(importStatus)
        //        .Then(_fixture.Create<BuildingId>(),
        //            importStatus.ToLegacyEvent()));
        //}

        ////TBD: Ruben question
        //[Fact]
        //public void WithDeletedWhenRetired()
        //{
        //    var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
        //        .WithCrabModification(CrabModification.Delete);

        //    Assert(new Scenario()
        //        .Given(_fixture.Create<BuildingId>(),
        //            _fixture.Create<BuildingWasRegistered>(),
        //            _fixture.Create<BuildingWasRetired>()
        //                .WithNoRetiredUnits())
        //        .When(importStatus)
        //        .Then(_fixture.Create<BuildingId>(),
        //            new BuildingVersionWasIncreased(_fixture.Create<BuildingId>(), new Version(1)),
        //            new BuildingWasNotRealized(_fixture.Create<BuildingId>(), new List<BuildingUnitId>(), new List<BuildingUnitId>()),
        //            importStatus.ToLegacyEvent()));
        //}
    }
}
