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
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
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
                    var buildingDetailItemV2 = await ct.BuildingDetailsV2.FindAsync(buildingWasMigrated.BuildingPersistentLocalId);
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
                    var buildingDetailItemV2 = await ct.BuildingDetailsV2.FindAsync(buildingWasPlannedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();

                    buildingDetailItemV2.IsRemoved.Should().BeFalse();
                    buildingDetailItemV2.Status.Should().Be(BuildingStatus.Planned);
                    buildingDetailItemV2.Version.Should().Be(buildingWasPlannedV2.Provenance.Timestamp);
                    buildingDetailItemV2.Geometry.Should().BeEquivalentTo(buildingWasPlannedV2.ExtendedWkbGeometry.ToByteArray());
                    buildingDetailItemV2.GeometryMethod.Should().Be(BuildingGeometryMethod.Outlined);
                    buildingDetailItemV2.LastEventHash.Should().Be(buildingWasPlannedV2.GetHash());
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasPlanned()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, new Dictionary<string, object>
                        {
                            { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
                        })),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, metadata)))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingDetailsV2.FindAsync(buildingUnitWasPlannedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2.Version.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                    buildingDetailItemV2.LastEventHash.Should().Be(buildingUnitWasPlannedV2.GetHash());
                });
        }

        [Fact]
        public async Task WhenCommonBuildingUnitWasAdded()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();

            var commonBuildingUnitWasAddedV2 = _fixture.Create<CommonBuildingUnitWasAddedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, commonBuildingUnitWasAddedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
                    })),
                    new Envelope<CommonBuildingUnitWasAddedV2>(new Envelope(commonBuildingUnitWasAddedV2, metadata)))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingDetailsV2.FindAsync(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2.Version.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                    buildingDetailItemV2.LastEventHash.Should().Be(commonBuildingUnitWasAddedV2.GetHash());
                });
        }

        [Fact]
        public async Task WhenBuildingOutlineWasChanged()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingOutlineWasChanged = _fixture.Create<BuildingOutlineWasChanged>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingOutlineWasChanged>(
                        new Envelope(
                            buildingOutlineWasChanged,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingOutlineWasChanged.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingDetailsV2.FindAsync(buildingOutlineWasChanged.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Version.Should().Be(buildingOutlineWasChanged.Provenance.Timestamp);

                    buildingDetailItemV2.Geometry.Should().BeEquivalentTo(buildingOutlineWasChanged.ExtendedWkbGeometryBuilding.ToByteArray());
                });
        }

        [Fact]
        public async Task WhenBuildingBecameUnderConstructionV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingBecameUnderConstructionV2 = _fixture.Create<BuildingBecameUnderConstructionV2>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
                    })),
                    new Envelope<BuildingBecameUnderConstructionV2>(new Envelope(buildingBecameUnderConstructionV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingBecameUnderConstructionV2.GetHash() }
                    })))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingDetailsV2.FindAsync(buildingBecameUnderConstructionV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2.Version.Should().Be(buildingBecameUnderConstructionV2.Provenance.Timestamp);
                    buildingDetailItemV2.Status.Should().Be(BuildingStatus.UnderConstruction);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromUnderConstructionToPlanned()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasCorrectedFromUnderConstructionToPlanned = _fixture.Create<BuildingWasCorrectedFromUnderConstructionToPlanned>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
                    })),
                    new Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>(new Envelope(buildingWasCorrectedFromUnderConstructionToPlanned, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromUnderConstructionToPlanned.GetHash() }
                    })))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingDetailsV2.FindAsync(buildingWasCorrectedFromUnderConstructionToPlanned.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2.Version.Should().Be(buildingWasCorrectedFromUnderConstructionToPlanned.Provenance.Timestamp);
                    buildingDetailItemV2.Status.Should().Be(BuildingStatus.Planned);
                });
        }

        [Fact]
        public async Task WhenBuildingWasRealizedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRealizedV2 = _fixture.Create<BuildingWasRealizedV2>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingWasRealizedV2>(
                        new Envelope(
                            buildingWasRealizedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasRealizedV2.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingDetailsV2.FindAsync(buildingWasRealizedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Status.Should().Be(BuildingStatus.Realized);
                    buildingDetailItemV2.Version.Should().Be(buildingWasRealizedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromRealizedToUnderConstruction()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRealizedV2 = _fixture.Create<BuildingWasRealizedV2>();
            var buildingWasCorrectedFromRealizedToUnderConstruction = _fixture.Create<BuildingWasCorrectedFromRealizedToUnderConstruction>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingWasRealizedV2>(
                        new Envelope(
                            buildingWasRealizedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasRealizedV2.GetHash() } })),
                    new Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>(
                        new Envelope(
                            buildingWasCorrectedFromRealizedToUnderConstruction,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromRealizedToUnderConstruction.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingDetailsV2.FindAsync(buildingWasRealizedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Status.Should().Be(BuildingStatus.UnderConstruction);
                    buildingDetailItemV2.Version.Should().Be(buildingWasCorrectedFromRealizedToUnderConstruction.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasNotRealizedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasNotRealizedV2 = _fixture.Create<BuildingWasNotRealizedV2>();

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
                    })),
                    new Envelope<BuildingWasNotRealizedV2>(new Envelope(buildingWasNotRealizedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingWasNotRealizedV2.GetHash() }
                    })))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingDetailsV2.FindAsync(buildingWasNotRealizedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2.Version.Should().Be(buildingWasNotRealizedV2.Provenance.Timestamp);
                    buildingDetailItemV2.Status.Should().Be(BuildingStatus.NotRealized);
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromNotRealizedToPlanned()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasNotRealizedV2 = _fixture.Create<BuildingWasNotRealizedV2>();
            var buildingWasCorrectedFromNotRealizedToPlanned = _fixture.Create<BuildingWasCorrectedFromNotRealizedToPlanned>();

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingWasNotRealizedV2>(
                        new Envelope(
                            buildingWasNotRealizedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasNotRealizedV2.GetHash() } })),
                    new Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>(
                        new Envelope(
                            buildingWasCorrectedFromNotRealizedToPlanned,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromNotRealizedToPlanned.GetHash() } })))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingDetailsV2.FindAsync(buildingWasNotRealizedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2!.Status.Should().Be(BuildingStatus.Planned);
                    buildingDetailItemV2.Version.Should().Be(buildingWasCorrectedFromNotRealizedToPlanned.Provenance.Timestamp);
                });
        }

        protected override BuildingDetailV2Projections CreateProjection() => new BuildingDetailV2Projections();
    }
}
