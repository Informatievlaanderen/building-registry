namespace BuildingRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Fixtures;
    using FluentAssertions;
    using Projections.Legacy.BuildingDetailV2;
    using Tests.Legacy.Autofixture;
    using Xunit;

    public class BuildingV2Tests : BuildingLegacyProjectionTest<BuildingDetailV2Projections>
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
                    var buildingDetailItemV2 = (await ct.BuildingDetailsV2.FindAsync(buildingWasMigrated.BuildingPersistentLocalId));
                    buildingDetailItemV2.Should().NotBeNull();

                    buildingDetailItemV2.IsRemoved.Should().Be(buildingWasMigrated.IsRemoved);
                    buildingDetailItemV2.Status.Value.Should().Be(buildingWasMigrated.BuildingStatus);
                    buildingDetailItemV2.Version.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                    buildingDetailItemV2.Geometry.Should().BeEquivalentTo(buildingWasMigrated.ExtendedWkbGeometry.ToByteArray());
                    buildingDetailItemV2.GeometryMethod.Value.Should().Be(buildingWasMigrated.GeometryMethod);
                    buildingDetailItemV2.LastEventHash.Should().Be(buildingWasMigrated.GetHash());
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
                    var buildingDetailItemV2 = (await ct.BuildingDetailsV2.FindAsync(buildingWasPlannedV2.BuildingPersistentLocalId));
                    buildingDetailItemV2.Should().NotBeNull();

                    buildingDetailItemV2.IsRemoved.Should().BeFalse();
                    buildingDetailItemV2.Status.Should().Be(BuildingStatus.Planned);
                    buildingDetailItemV2.Version.Should().Be(buildingWasPlannedV2.Provenance.Timestamp);
                    buildingDetailItemV2.Geometry.Should().BeEquivalentTo(buildingWasPlannedV2.ExtendedWkbGeometry.ToByteArray());
                    buildingDetailItemV2.GeometryMethod.Should().Be(BuildingGeometryMethod.Outlined);
                    buildingDetailItemV2.LastEventHash.Should().Be(buildingWasPlannedV2.GetHash());
                });
        }

        protected override BuildingDetailV2Projections CreateProjection() => new BuildingDetailV2Projections();
    }
}