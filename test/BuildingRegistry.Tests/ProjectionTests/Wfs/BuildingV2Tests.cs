namespace BuildingRegistry.Tests.ProjectionTests.Wfs
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Autofixture;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using FluentAssertions;
    using Infrastructure;
    using Projections.Wfs.BuildingV2;
    using Tests.Legacy.Autofixture;
    using Xunit;
    using Polygon = NetTopologySuite.Geometries.Polygon;

    public class BuildingV2Tests : BuildingWfsProjectionTest<BuildingV2Projections>
    {
        private readonly Fixture? _fixture;
        
        public BuildingV2Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithBuildingStatus());
            _fixture.Customize(new WithBuildingGeometryMethod());
            _fixture.Customize(new WithValidExtendedWkbPolygon());
            _fixture.Customize(new WithBuildingUnitStatus());
            _fixture.Customize(new WithBuildingUnitFunction());
            _fixture.Customize(new WithBuildingUnitPositionGeometryMethod());
        }

        [Fact]
        public async Task WhenBuildingWasMigrated()
        {
            var buildingWasMigrated = _fixture.Create<BuildingWasMigrated>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingWasMigrated>(new Envelope(buildingWasMigrated, metadata)))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = (await ct.BuildingsV2.FindAsync(buildingWasMigrated.BuildingPersistentLocalId));
                    buildingDetailItemV2.Should().NotBeNull();

                    buildingDetailItemV2.Id.Should().Be(PersistentLocalIdHelper.CreateBuildingId(buildingWasMigrated.BuildingPersistentLocalId));
                    buildingDetailItemV2.IsRemoved.Should().Be(buildingWasMigrated.IsRemoved);
                    buildingDetailItemV2.Status.Should().Be(BuildingV2Projections.MapStatus(BuildingStatus.Parse(buildingWasMigrated.BuildingStatus)));
                    buildingDetailItemV2.Version.Should().Be(buildingWasMigrated.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(buildingWasMigrated.ExtendedWkbGeometry.ToByteArray()) as Polygon;
                    buildingDetailItemV2.Geometry.Should().Be(new GrbPolygon(polygon));
                    buildingDetailItemV2.GeometryMethod.Should().Be(BuildingV2Projections.MapGeometryMethod(BuildingGeometryMethod.Parse(buildingWasMigrated.GeometryMethod)));
                });
        }

        [Fact]
        public async Task WhenBuildingWasPlanned()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, metadata)))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = (await ct.BuildingsV2.FindAsync(buildingWasPlannedV2.BuildingPersistentLocalId));
                    buildingDetailItemV2.Should().NotBeNull();

                    buildingDetailItemV2.Id.Should().Be(PersistentLocalIdHelper.CreateBuildingId(buildingWasPlannedV2.BuildingPersistentLocalId));
                    buildingDetailItemV2.IsRemoved.Should().BeFalse();
                    buildingDetailItemV2.Status.Should().Be(BuildingV2Projections.MapStatus(BuildingStatus.Planned));
                    buildingDetailItemV2.Version.Should().Be(buildingWasPlannedV2.Provenance.Timestamp);

                    var wkbReader = WKBReaderFactory.Create();
                    var polygon = wkbReader.Read(buildingWasPlannedV2.ExtendedWkbGeometry.ToByteArray());
                    buildingDetailItemV2.Geometry.AsBinary().Should().BeEquivalentTo(polygon.AsBinary());
                    buildingDetailItemV2.GeometryMethod.Should().Be(BuildingV2Projections.MapGeometryMethod(BuildingGeometryMethod.Outlined));
                });
        }

        protected override BuildingV2Projections CreateProjection() => new BuildingV2Projections(WKBReaderFactory.Create());
    }
}
