namespace BuildingRegistry.Tests.WhenImportingCrabSubaddressPosition
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Building.Commands.Crab;
    using Building.DataStructures;
    using Building.Events;
    using Building.Events.Crab;
    using NetTopologySuite.IO;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithGeometryAndUnit : SnapshotBasedTest
    {
        public GivenBuildingWithGeometryAndUnit(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithInfiniteLifetime());
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithFixedBuildingUnitIdFromSubaddress());
            Fixture.Customize(new WithValidPoint());
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

            var command = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithPositionOrigin(crabAddressPosition);
            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitWasAdded>())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasAppointedByAdministrator(buildingId, Fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(Fixture.Create<WkbGeometry>())),
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

            var command = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithPositionOrigin(crabAddressPosition);
            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitWasAdded>())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, Fixture.Create<BuildingUnitId>(), ExtendedWkbGeometry.CreateEWkb(new WkbGeometry(derived.AsBinary()))),
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

            var command = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithPositionOrigin(crabAddressPosition);
            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitWasAdded>())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, Fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(Fixture.Create<WkbGeometry>())),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithValidGeometryWhenBuildingHasGeometryAfterUnitWasAdded()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());

            var command = Fixture.Create<ImportSubaddressPositionFromCrab>();
            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()))
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasAppointedByAdministrator(buildingId, Fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(Fixture.Create<WkbGeometry>())),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithGeometryOutsideBuilding()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            Fixture.Customize(new WithInvalidPoint());

            var derived = new WKBReader().Read(polygonFixture.Create<WkbGeometry>()).Centroid;

            var command = Fixture.Create<ImportSubaddressPositionFromCrab>();
            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()))
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, Fixture.Create<BuildingUnitId>(), ExtendedWkbGeometry.CreateEWkb(new WkbGeometry(derived.AsBinary()))),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WhenModificationIsDelete()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var command = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithModification(CrabModification.Delete);
            var buildingId = Fixture.Create<BuildingId>();

            var center = new WKBReader().Read(polygonFixture.Create<WkbGeometry>()).Centroid;

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitPositionWasDerivedFromObject>()
                        .WithGeometry(new WkbGeometry(center.AsBinary())))
                .When(command)
                .Then(buildingId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void WithUnitGeometryWhenModificationIsDelete()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var command = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithModification(CrabModification.Delete);
            var buildingId = Fixture.Create<BuildingId>();

            var center = new WKBReader().Read(polygonFixture.Create<WkbGeometry>()).Centroid;

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitPositionWasDerivedFromObject>())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, Fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(new WkbGeometry(center.AsBinary()))),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void ThenNoPositionChangeWhenPositionIsTheSame()
        {
            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var command = Fixture.Create<ImportSubaddressPositionFromCrab>().WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot);
            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>()
                        .WithGeometry(command.AddressPosition),
                    command.ToLegacyEvent())
                .When(command)
                .Then(buildingId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void ThenAddressWasPositionedWhenNewerLifetimeAndHigherQuality()
        {
            var addressSubaddressPositionWasImportedFromCrab = Fixture.Create<AddressSubaddressPositionWasImportedFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot);

            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var newPosition = new WkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary());
            var command = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithPosition(newPosition)
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithLifetime(new CrabLifetime(addressSubaddressPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(1), addressSubaddressPositionWasImportedFromCrab.EndDateTime));

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>(),
                    addressSubaddressPositionWasImportedFromCrab)
                .When(command)
                .Then(buildingId,
                    new BuildingUnitPositionWasAppointedByAdministrator(buildingId, Fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(newPosition)),
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void NoPositionedWhenNewerLifetimeAndLowerQuality()
        {
            var addressSubaddressPositionWasImportedFromCrab = Fixture.Create<AddressSubaddressPositionWasImportedFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand);

            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var newPosition = new WkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary());
            var command = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithPosition(newPosition)
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithLifetime(new CrabLifetime(addressSubaddressPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(1), addressSubaddressPositionWasImportedFromCrab.EndDateTime));

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>()
                        .WithGeometry(new WkbGeometry(addressSubaddressPositionWasImportedFromCrab.AddressPosition)),
                    addressSubaddressPositionWasImportedFromCrab)
                .When(command)
                .Then(buildingId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void ThenNoPositionChangeWhenOlderLifetimeAndLessQuality()
        {
            var addressSubaddressPositionWasImportedFromCrab = Fixture.Create<AddressSubaddressPositionWasImportedFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand);

            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var newPosition = new WkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary());
            var command = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithPosition(newPosition)
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot)
                .WithLifetime(new CrabLifetime(addressSubaddressPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(-1), addressSubaddressPositionWasImportedFromCrab.EndDateTime));

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingUnitWasAdded>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>()
                        .WithGeometry(new WkbGeometry(addressSubaddressPositionWasImportedFromCrab.AddressPosition)),
                    addressSubaddressPositionWasImportedFromCrab)
                .When(command)
                .Then(buildingId,
                    command.ToLegacyEvent()));
        }

        [Fact]
        public void ThenPositionChangeWhenOlderLifetimeAndHigherQuality()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var addressSubaddressPositionWasImportedFromCrab = Fixture.Create<AddressSubaddressPositionWasImportedFromCrab>()
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromLot);

            var polygonFixture = new Fixture().Customize(new WithValidPolygon());
            var newPosition = new WkbGeometry(GeometryHelper.OtherValidPointInPolygon.AsBinary());
            var command = Fixture.Create<ImportSubaddressPositionFromCrab>()
                .WithPosition(newPosition)
                .WithPositionOrigin(CrabAddressPositionOrigin.ManualIndicationFromStand)
                .WithLifetime(new CrabLifetime(addressSubaddressPositionWasImportedFromCrab.BeginDateTime.Value.PlusDays(-1), addressSubaddressPositionWasImportedFromCrab.EndDateTime));

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAdded = Fixture.Create<BuildingUnitWasAdded>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    buildingUnitWasAdded,
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(polygonFixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingUnitPositionWasAppointedByAdministrator>(),
                    addressSubaddressPositionWasImportedFromCrab)
                .When(command)
                .Then(new Fact[]
                {
                    new Fact(buildingId, new BuildingUnitPositionWasAppointedByAdministrator(buildingId, Fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(newPosition))),
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId), BuildingSnapshotBuilder.CreateDefaultSnapshot(buildingId)
                        .WithGeometry(new BuildingGeometry(GeometryHelper.CreateEwkbFrom(polygonFixture.Create<WkbGeometry>()), BuildingGeometryMethod.MeasuredByGrb))
                        .WithSubaddressPositionEventsByHouseNumberId(new Dictionary<CrabSubaddressId, List<AddressSubaddressPositionWasImportedFromCrab>>
                        {
                            { new CrabSubaddressId(addressSubaddressPositionWasImportedFromCrab.SubaddressId), new List<AddressSubaddressPositionWasImportedFromCrab>{ addressSubaddressPositionWasImportedFromCrab, command.ToLegacyEvent() } },
                        })
                        .WithLastModificationFromCrab(Modification.Update)
                        .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                            .WithBuildingUnits(new List<BuildingUnitSnapshot>
                            {
                                BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                    .WithSubaddressPositions(new List<AddressSubaddressPositionWasImportedFromCrab> { addressSubaddressPositionWasImportedFromCrab, command.ToLegacyEvent() })
                                    .WithPosition(new BuildingUnitPosition(GeometryHelper.CreateEwkbFrom(newPosition), BuildingUnitPositionGeometryMethod.AppointedByAdministrator)),
                            }))
                        .Build(6, EventSerializerSettings))
                }));
        }

        // see AddressRegistry for AddressComparerQuality tests
    }
}
