namespace BuildingRegistry.Tests.WhenImportingCrabHouseNumberPosition
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Building.Commands.Crab;
    using Building.Events;
    using Building.Events.Crab;
    using NetTopologySuite.IO;
    using ValueObjects;
    using ValueObjects.Crab;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithGeometryAndUnit : AutofacBasedTest
    {
        private readonly Fixture _fixture = new Fixture();

        public GivenBuildingWithGeometryAndUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithInfiniteLifetime());
            _fixture.Customize(new WithNoDeleteModification());
            _fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
            _fixture.Customize(new WithValidPoint());
        }

        [Theory]
        [InlineData(CrabAddressPositionOrigin.ManualIndicationFromBerth)]
        [InlineData(CrabAddressPositionOrigin.ManualIndicationFromEntryOfBuilding)]
        [InlineData(CrabAddressPositionOrigin.ManualIndicationFromLot)]
        [InlineData(CrabAddressPositionOrigin.ManualIndicationFromParcel)]
        [InlineData(CrabAddressPositionOrigin.ManualIndicationFromBuilding)]
        [InlineData(CrabAddressPositionOrigin.ManualIndicationFromStand)]
        public void WithValidManualGeometryForCrabOrigin(CrabAddressPositionOrigin crabAddressPosition)
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());

            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithPositionOrigin(crabAddressPosition);
            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitWasAdded>())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasAppointedByAdministrator(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(_fixture.Create<WkbGeometry>())),
                    command.ToLegacyEvent()));
        }

        [Theory]
        [InlineData(CrabAddressPositionOrigin.ManualIndicationFromAccessToTheRoad)]
        [InlineData(CrabAddressPositionOrigin.ManualIndicationFromUtilityConnection)]
        [InlineData(CrabAddressPositionOrigin.ManualIndicationFromMailbox)]
        public void WithManualApplicationGeometryOrigin(CrabAddressPositionOrigin crabAddressPosition)
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());

            var derived = new WKBReader().Read(polygonFixture.Create<WkbGeometry>()).Centroid;

            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithPositionOrigin(crabAddressPosition);
            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitWasAdded>())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(new WkbGeometry(derived.AsBinary()))),
                    command.ToLegacyEvent()));

        }

        [Theory]
        [InlineData(CrabAddressPositionOrigin.DerivedFromBuilding)]
        [InlineData(CrabAddressPositionOrigin.DerivedFromMunicipality)]
        [InlineData(CrabAddressPositionOrigin.DerivedFromParcelCadastre)]
        [InlineData(CrabAddressPositionOrigin.DerivedFromParcelGrb)]
        [InlineData(CrabAddressPositionOrigin.DerivedFromStreet)]
        [InlineData(CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersBuilding)]
        [InlineData(CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelCadastre)]
        [InlineData(CrabAddressPositionOrigin.InterpolatedBasedOnAdjacentHouseNumbersParcelGrb)]
        [InlineData(CrabAddressPositionOrigin.InterpolatedBasedOnRoadConnection)]
        public void WithValidDerivedGeometryForCrabOrigin(CrabAddressPositionOrigin crabAddressPosition)
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());

            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithPositionOrigin(crabAddressPosition);
            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitWasAdded>())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(_fixture.Create<WkbGeometry>())),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithValidGeometryWhenBuildingHasGeometryAfterUnitWasAdded()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());

            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>();
            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()))
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasAppointedByAdministrator(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(_fixture.Create<WkbGeometry>())),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithGeometryOutsideBuilding()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            _fixture.Customize(new WithInvalidPoint());

            var derived = new WKBReader().Read(polygonFixture.Create<WkbGeometry>()).Centroid;

            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>();
            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()))
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(new WkbGeometry(derived.AsBinary()))),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenModificationIsDelete()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithModification(CrabModification.Delete);
            var buildingId = _fixture.Create<BuildingId>();

            var center = new WKBReader().Read(polygonFixture.Create<WkbGeometry>()).Centroid;

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitPositionWasDerivedFromObject>()
                        .WithGeometry(new WkbGeometry(center.AsBinary())))
                .When(command)
                .Then(buildingId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithUnitGeometryWhenModificationIsDelete()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithModification(CrabModification.Delete);
            var buildingId = _fixture.Create<BuildingId>();

            var center = new WKBReader().Read(polygonFixture.Create<WkbGeometry>()).Centroid;

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitPositionWasDerivedFromObject>())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(new WkbGeometry(center.AsBinary()))),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void ThenNoPositionChangeWhenPositionIsTheSame()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>().WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromBuilding);
            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>()
                        .WithGeometry(command.AddressPosition),
                    command.ToLegacyEvent())
                .When(command)
                .Then(buildingId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithValidGeometryWhenBuildingGeometryRemoved()
        {
            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>().WithPositionOrigin(CrabAddressPositionOrigin.DerivedFromBuilding);

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<AddressHouseNumberPositionWasImportedFromCrab>()
                        .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand),
                    _fixture.Create<BuildingWasMeasuredByGrb>(),
                    _fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>(),
                    _fixture.Create<BuildingGeometryWasRemoved>())
                .When(command)
                .Then(buildingId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void ThenAddressWasPositionedWhenNewerLifetimeAndHigherQuality()
        {
            var addressHouseNumberPositionWasImportedFromCrab = _fixture.Create<AddressHouseNumberPositionWasImportedFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot);

            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var newPosition = new WkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary());
            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithPosition(newPosition)
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithLifetime(new CrabLifetime(addressHouseNumberPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(1), addressHouseNumberPositionWasImportedFromCrab.EndDateTime));

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>(),
                    addressHouseNumberPositionWasImportedFromCrab)
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasAppointedByAdministrator(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(newPosition)),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void NoPositionedWhenNewerLifetimeAndLowerQuality()
        {
            var addressHouseNumberPositionWasImportedFromCrab = _fixture.Create<AddressHouseNumberPositionWasImportedFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand);

            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var newPosition = new WkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary());
            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithPosition(newPosition)
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithLifetime(new CrabLifetime(addressHouseNumberPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(1), addressHouseNumberPositionWasImportedFromCrab.EndDateTime));

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>()
                        .WithGeometry(new WkbGeometry(addressHouseNumberPositionWasImportedFromCrab.AddressPosition)),
                    addressHouseNumberPositionWasImportedFromCrab)
                .When(command)
                .Then(buildingId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void ThenNoPositionChangeWhenOlderLifetimeAndLessQuality()
        {
            var addressHouseNumberPositionWasImportedFromCrab = _fixture.Create<AddressHouseNumberPositionWasImportedFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand);

            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var newPosition = new WkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary());
            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithPosition(newPosition)
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithLifetime(new CrabLifetime(addressHouseNumberPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(-1), addressHouseNumberPositionWasImportedFromCrab.EndDateTime));

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>()
                        .WithGeometry(new WkbGeometry(addressHouseNumberPositionWasImportedFromCrab.AddressPosition)),
                    addressHouseNumberPositionWasImportedFromCrab)
                .When(command)
                .Then(buildingId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void ThenPositionChangeWhenOlderLifetimeAndHigherQuality()
        {
            var addressHouseNumberPositionWasImportedFromCrab = _fixture.Create<AddressHouseNumberPositionWasImportedFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot);

            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var newPosition = new WkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary());
            var command = _fixture.Create<ImportHouseNumberPositionFromCrab>()
                .WithPosition(newPosition)
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithLifetime(new CrabLifetime(addressHouseNumberPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(-1), addressHouseNumberPositionWasImportedFromCrab.EndDateTime));

            var buildingId = _fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    _fixture.Create<BuildingWasRegistered>(),
                    _fixture.Create<BuildingUnitWasAdded>(),
                    _fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    _fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>(),
                    addressHouseNumberPositionWasImportedFromCrab)
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasAppointedByAdministrator(buildingId, _fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(newPosition)),
                    command.ToLegacyEvent()));
        }

        // see AddressRegistry for AddressComparerQuality tests
    }
}
