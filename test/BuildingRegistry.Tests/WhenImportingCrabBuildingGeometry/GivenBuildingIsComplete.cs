namespace BuildingRegistry.Tests.WhenImportingCrabBuildingGeometry
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
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingIsComplete : SnapshotBasedTest
    {
        public GivenBuildingIsComplete(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithValidPolygon());
        }

        [Fact]
        public void ThenBuildingBecameIncompleteWhenModificationIsDelete()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var buildingId = Fixture.Create<BuildingId>();

            var importGeometry = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithCrabModification(CrabModification.Delete);

            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingWasMeasuredByGrb>()
                        .WithGeometry(new WkbGeometry(GeometryHelper.ValidPolygon.AsBinary())),
                    Fixture.Create<BuildingBecameUnderConstruction>(),
                    Fixture.Create<BuildingBecameComplete>())
                .When(importGeometry)
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingGeometryWasRemoved(buildingId)),
                    new Fact(buildingId, new BuildingBecameIncomplete(buildingId)),
                    new Fact(buildingId, importGeometry.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithGeometryChronicle(importGeometry)
                            .WithStatus(BuildingStatus.UnderConstruction)
                            .BecameComplete(false)
                            .WithIsRemoved(false)
                            .WithLastModificationFromCrab(Modification.Insert)
                            .Build(6, EventSerializerSettings))
                }));
        }
    }
}
