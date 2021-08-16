namespace BuildingRegistry.Tests.WhenImportingCrabBuildingGeometry
{
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
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

    public class GivenBuildingHasStatus : SnapshotBasedTest
    {

        public GivenBuildingHasStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingId());
            Fixture.Customize(new WithNoDeleteModification());
            Fixture.Customize(new WithValidPolygon());
        }

        [Fact]
        public void ThenBuildingBecameCompleteWhenModificationIsDelete()
        {
            Fixture.Customize(new WithSnapshotInterval(1));
            var importBuilding = Fixture.Create<ImportBuildingGeometryFromCrab>()
                .WithGeometryMethod(CrabBuildingGeometryMethod.Grb);

            var buildingId = Fixture.Create<BuildingId>();
            var a = new BuildingWasMeasuredByGrb(buildingId,
                GeometryHelper.CreateEwkbFrom(importBuilding.BuildingGeometry));
            Assert(new Scenario()
                .Given(buildingId,
                    Fixture.Create<BuildingWasRegistered>(),
                    Fixture.Create<BuildingBecameUnderConstruction>())
                .When(importBuilding)
                .Then(new[]
                {
                    new Fact(buildingId, new BuildingWasMeasuredByGrb(buildingId, GeometryHelper.CreateEwkbFrom(importBuilding.BuildingGeometry))),
                    new Fact(buildingId, new BuildingBecameComplete(buildingId)),
                    new Fact(buildingId, importBuilding.ToLegacyEvent()),
                    new Fact(GetSnapshotIdentifier(buildingId),
                        BuildingSnapshotBuilder
                            .CreateDefaultSnapshot(buildingId)
                            .WithGeometry(new BuildingGeometry(GeometryHelper.CreateEwkbFrom(importBuilding.BuildingGeometry), BuildingGeometryMethod.MeasuredByGrb))
                            .WithGeometryChronicle(importBuilding)
                            .WithStatus(BuildingStatus.UnderConstruction)
                            .BecameComplete(true)
                            .WithLastModificationFromCrab(Modification.Insert)
                            .Build(4, EventSerializerSettings))
                }));
        }
    }
}
