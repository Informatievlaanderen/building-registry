namespace BuildingRegistry.Tests.WhenImportingCrabTerrainObjectHouseNumber
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Building.Commands.Crab;
    using Building.DataStructures;
    using Building.Events;
    using NetTopologySuite.IO;
    using ValueObjects;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingWithGeometry : SnapshotBasedTest
    {
        public GivenBuildingWithGeometry(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithInfiniteLifetime());
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithValidPolygon());
            Fixture.Customize(new WithFixedBuildingUnitIdFromHouseNumber());
        }

        [Fact]
        public void ThenBuildingUnitIsAddedWithDerivedGeometry()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var center = new WKBReader().Read(Fixture.Create<WkbGeometry>()).Centroid;

            var buildingId = Fixture.Create<BuildingId>();
            var buildingUnitWasAdded = new BuildingUnitWasAdded(buildingId, Fixture.Create<BuildingUnitId>(), Fixture.Create<BuildingUnitKey>(), AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp));

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(Fixture.Create<WkbGeometry>()))
                .When(command)
                .Then(new Fact[] {
                    new Fact(buildingId, buildingUnitWasAdded),
                    new Fact(buildingId, new BuildingUnitPositionWasDerivedFromObject(buildingId, Fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(new WkbGeometry(center.AsBinary())))),
                    new Fact(buildingId, command.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder.CreateDefaultSnapshot(buildingId)
                            .WithImportedTerrainObjectHouseNrIds(new List<CrabTerrainObjectHouseNumberId>{command.TerrainObjectHouseNumberId})
                            .WithGeometry(new BuildingGeometry(GeometryHelper.CreateEwkbFrom(Fixture.Create<WkbGeometry>()), BuildingGeometryMethod.MeasuredByGrb))
                            .WithActiveHouseNumberIdsByTerrainObjectHouseNr(new Dictionary<CrabTerrainObjectHouseNumberId, CrabHouseNumberId>
                            {
                                { command.TerrainObjectHouseNumberId, command.HouseNumberId }
                            })
                            .WithBuildingUnitCollection(BuildingUnitCollectionSnapshotBuilder.CreateDefaultSnapshot()
                                .WithBuildingUnits(new List<BuildingUnitSnapshot>
                                {
                                    BuildingUnitSnapshotBuilder.CreateDefaultSnapshotFor(buildingUnitWasAdded)
                                        .WithPosition(new BuildingUnitPosition(GeometryHelper.CreateEwkbFrom(new WkbGeometry(center.AsBinary())), BuildingUnitPositionGeometryMethod.DerivedFromObject))
                                }))
                            .Build(4, EventSerializerSettings))
                }));
        }

        [Fact]
        public void WithFiniteLifetimeThenRetireNewlyAddedBuildingUnitAndDerivePosition()
        {
            var command = Fixture.Create<ImportTerrainObjectHouseNumberFromCrab>();

            var center = new WKBReader().Read(Fixture.Create<WkbGeometry>()).Centroid;

            var buildingId = Fixture.Create<BuildingId>();

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                                .WithGeometry(Fixture.Create<WkbGeometry>()),
                    Fixture.Create<BuildingWasNotRealized>()
                        .WithNoRetiredUnits())
                .When(command)
                .Then(buildingId,
                    new BuildingUnitWasAddedToRetiredBuilding(buildingId, Fixture.Create<BuildingUnitId>(), Fixture.Create<BuildingUnitKey>(), AddressId.CreateFor(command.HouseNumberId), new BuildingUnitVersion(command.Timestamp)),
                    new BuildingUnitPositionWasDerivedFromObject(buildingId, Fixture.Create<BuildingUnitId>(), GeometryHelper.CreateEwkbFrom(new WkbGeometry(center.AsBinary()))),
                    command.ToLegacyEvent()));
        }
    }
}
