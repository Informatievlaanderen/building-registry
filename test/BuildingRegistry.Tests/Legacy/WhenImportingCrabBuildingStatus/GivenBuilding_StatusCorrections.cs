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

    public class GivenBuildingStatusCorrections : AutofacBasedTest
    {
        private readonly Fixture _fixture;

        public GivenBuildingStatusCorrections(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingId());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithInfiniteLifetime());
        }

        [Fact]
        public void WhenStatusIsPermitRequestedAndIsCorrection()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.PermitRequested)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasCorrectedToPlanned(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenStatusIsPermitGrantedAndIsCorrection()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.BuildingPermitGranted)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasCorrectedToPlanned(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenStatusIsInUseAndIsCorrection()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.InUse)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasCorrectedToRealized(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenStatusIsUnderConstructionAndIsCorrection()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.UnderConstruction)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasCorrectedToUnderConstruction(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }

        [Fact]
        public void WhenStatusIsOutOfUseAndIsCorrection()
        {
            var importStatus = _fixture.Create<ImportBuildingStatusFromCrab>()
                .WithStatus(CrabBuildingStatus.OutOfUse)
                .WithCrabModification(CrabModification.Correction);

            Assert(new Scenario()
                .Given(_fixture.Create<BuildingId>(),
                    _fixture.Create<BuildingWasRegistered>())
                .When(importStatus)
                .Then(_fixture.Create<BuildingId>(),
                    new BuildingWasCorrectedToRealized(_fixture.Create<BuildingId>()),
                    importStatus.ToLegacyEvent()));
        }
    }
}
