namespace BuildingRegistry.Tests.ProjectionTests.Wfs
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Fixtures;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Projections.Wfs.BuildingUnitV2;
    using Tests.Legacy.Autofixture;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    public class BuildingUnitV2Tests : BuildingWfsProjectionTest<BuildingUnitV2Projections>
    {
        private readonly WKBReader _wkbReader;
        private readonly Fixture _fixture = new Fixture();

        public BuildingUnitV2Tests()
        {
            _wkbReader = WKBReaderFactory.Create();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithBuildingStatus());
            _fixture.Customize(new WithBuildingGeometryMethod());
            _fixture.Customize(new WithValidExtendedWkbPoint());
            _fixture.Customize(new WithBuildingUnitStatus());
            _fixture.Customize(new WithBuildingUnitFunction());
            _fixture.Customize(new WithBuildingUnitPositionGeometryMethod());
        }

        [Theory]
        [InlineData("Planned", null)]
        [InlineData("UnderConstruction", null)]
        [InlineData("Realized", null)]
        [InlineData("Retired", "Retired")]
        [InlineData("NotRealized", "NotRealized")]
        public async Task WhenBuildingWasMigrated(string buildingStatus, string? expectedStatus)
        {
            _fixture.Register(() => BuildingStatus.Parse(buildingStatus));

            var buildingWasMigrated = _fixture.Create<BuildingWasMigrated>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingWasMigrated>(new Envelope(buildingWasMigrated, metadata)))
                .Then(async ct =>
                {
                    var buildingUnitBuildingItem = await ct.BuildingUnitsBuildingsV2.FindAsync(buildingWasMigrated.BuildingPersistentLocalId);
                    buildingUnitBuildingItem.Should().NotBeNull();

                    buildingUnitBuildingItem.IsRemoved.Should().Be(buildingWasMigrated.IsRemoved);
                    if (string.IsNullOrEmpty(expectedStatus))
                    {
                        buildingUnitBuildingItem.BuildingRetiredStatus.Should().BeNull();
                    }
                    else
                    {
                        buildingUnitBuildingItem.BuildingRetiredStatus.Value.Value.Should().Be(expectedStatus);
                    }

                    var buildingUnits = ct.BuildingUnitsV2
                        .Where(unit => unit.BuildingPersistentLocalId == buildingWasMigrated.BuildingPersistentLocalId)
                        .ToList();

                    foreach (var unit in buildingWasMigrated.BuildingUnits)
                    {
                        var expectedUnit = buildingUnits
                            .Single(x => x.BuildingUnitPersistentLocalId == unit.BuildingUnitPersistentLocalId);

                        expectedUnit.BuildingPersistentLocalId.Should().Be(buildingWasMigrated.BuildingPersistentLocalId);
                        expectedUnit.IsRemoved.Should().Be(unit.IsRemoved);
                        expectedUnit.Status.Should().Be(BuildingUnitV2Projections.MapStatus(BuildingUnitStatus.Parse(unit.Status)));
                        expectedUnit.Function.Should().Be(BuildingUnitV2Projections.MapFunction(BuildingUnitFunction.Parse(unit.Function)));
                        expectedUnit.PositionMethod.Should().Be(BuildingUnitV2Projections.MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(unit.GeometryMethod)));
                        expectedUnit.Version.Should().Be(buildingWasMigrated.Provenance.Timestamp);

                        var point = WKBReaderFactory.Create().Read(unit.ExtendedWkbGeometry.ToByteArray()) as Point;
                        expectedUnit.Position.Should().Be(point);
                    }
                });
        }

        [Fact]
        public async Task WhenBuildingWasPlannedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, metadata)))
                .Then(async ct =>
                {
                    var buildingDetailItemV2 = await ct.BuildingUnitsBuildingsV2.FindAsync(buildingWasPlannedV2.BuildingPersistentLocalId);
                    buildingDetailItemV2.Should().NotBeNull();
                    buildingDetailItemV2.BuildingRetiredStatus.Should().BeNull();
                    buildingDetailItemV2.IsRemoved.Should().BeFalse();
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasPlannedV2()
        {
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, metadata)))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitsV2.FindAsync(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();
                    item.BuildingPersistentLocalId.Should().Be(buildingUnitWasPlannedV2.BuildingPersistentLocalId);
                    item.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(buildingUnitWasPlannedV2.ExtendedWkbGeometry.ToByteArray()));
                    item.PositionMethod.Should().Be(BuildingUnitV2Projections.MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(buildingUnitWasPlannedV2.GeometryMethod)));
                    item.Function.Should()
                        .Be(BuildingUnitV2Projections.MapFunction(
                            BuildingUnitFunction.Parse(buildingUnitWasPlannedV2.Function)));
                    item.Version.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                    item.IsRemoved.Should().BeFalse();
                    item.Status.Should().Be("Gepland");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRealizedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            var @event = _fixture.Create<BuildingUnitWasRealizedV2>();

            await Sut
                .Given(new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2,
                        new Dictionary<string, object>
                        {
                            {AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash()}
                        })),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(@event, new Dictionary<string, object>
                    {
                        {AddEventHashPipe.HashMetadataKey, @event.GetHash()}
                    })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item.IsRemoved.Should().BeFalse();
                    item.Status.Should().Be("Gerealiseerd");
                });
        }

         [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRealizedToPlanned()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();

            var @event = _fixture.Create<BuildingUnitWasCorrectedFromRealizedToPlanned>();

            await Sut
                .Given(new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2,
                        new Dictionary<string, object>
                        {
                            {AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash()}
                        })),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(buildingUnitWasRealizedV2, new Dictionary<string, object>
                    {
                        {AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash()}
                    })),
                    new Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>(new Envelope(@event, new Dictionary<string, object>
                    {
                        {AddEventHashPipe.HashMetadataKey, @event.GetHash()}
                    })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item.IsRemoved.Should().BeFalse();
                    item.Status.Should().Be("Gepland");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();
            var @event = _fixture.Create<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { {AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash()} })),
                    new Envelope<BuildingUnitWasRealizedV2>(
                        new Envelope(
                            buildingUnitWasRealizedV2,
                            new Dictionary<string, object> { {AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash()} })),
                    new Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { {AddEventHashPipe.HashMetadataKey, @event.GetHash()} })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be("Gepland");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasNotRealizedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            var @event = _fixture.Create<BuildingUnitWasNotRealizedV2>();

            await Sut
                .Given(new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2,
                        new Dictionary<string, object>
                        {
                            {AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash()}
                        })),
                    new Envelope<BuildingUnitWasNotRealizedV2>(new Envelope(@event, new Dictionary<string, object>
                    {
                        {AddEventHashPipe.HashMetadataKey, @event.GetHash()}
                    })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item.IsRemoved.Should().BeFalse();
                    item.Status.Should().Be("NietGerealiseerd");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromNotRealizedToPlanned()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasNotRealizedV2 = _fixture.Create<BuildingUnitWasNotRealizedV2>();
            var @event = _fixture.Create<BuildingUnitWasCorrectedFromNotRealizedToPlanned>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasNotRealizedV2>(
                        new Envelope(
                            buildingUnitWasNotRealizedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasNotRealizedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be("Gepland");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRetiredV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();
            var @event = _fixture.Create<BuildingUnitWasRetiredV2>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasRealizedV2>(
                        new Envelope(
                            buildingUnitWasRealizedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasRetiredV2>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be("Gehistoreerd");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRetiredToRealized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRetiredV2 = _fixture.Create<BuildingUnitWasRetiredV2>();
            var @event = _fixture.Create<BuildingUnitWasCorrectedFromRetiredToRealized>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(
                            buildingUnitWasPlannedV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() } })),
                    new Envelope<BuildingUnitWasRetiredV2>(
                        new Envelope(
                            buildingUnitWasRetiredV2,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, buildingUnitWasRetiredV2.GetHash() } })),
                    new Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>(
                        new Envelope(
                            @event,
                            new Dictionary<string, object> { { AddEventHashPipe.HashMetadataKey, @event.GetHash() } })))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitsV2.FindAsync(@event.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();

                    item!.Status.Should().Be("Gerealiseerd");
                });
        }

        [Fact]
        public async Task WhenCommonBuildingUnitWasAddedV2()
        {
            var commonBuildingUnitWasAddedV2 = new CommonBuildingUnitWasAddedV2(
                _fixture.Create<BuildingPersistentLocalId>(),
                _fixture.Create<BuildingUnitPersistentLocalId>(),
                BuildingUnitStatus.Planned,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                _fixture.Create<ExtendedWkbGeometry>(),
                false);
            ((ISetProvenance)commonBuildingUnitWasAddedV2).SetProvenance(_fixture.Create<Provenance>());

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, commonBuildingUnitWasAddedV2.GetHash() }
            };

            await Sut
                .Given(new Envelope<CommonBuildingUnitWasAddedV2>(new Envelope(commonBuildingUnitWasAddedV2, metadata)))
                .Then(async ct =>
                {
                    var item = await ct.BuildingUnitsV2.FindAsync(commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId);
                    item.Should().NotBeNull();
                    item!.BuildingPersistentLocalId.Should().Be(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
                    item.Position.Should().BeEquivalentTo((Point)_wkbReader.Read(commonBuildingUnitWasAddedV2.ExtendedWkbGeometry.ToByteArray()));
                    item.PositionMethod.Should().Be(BuildingUnitV2Projections.MapGeometryMethod(BuildingUnitPositionGeometryMethod.Parse(commonBuildingUnitWasAddedV2.GeometryMethod)));
                    item.Function.Should().Be(GebouweenheidFunctie.GemeenschappelijkDeel.ToString());
                    item.Version.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                    item.IsRemoved.Should().BeFalse();
                    item.Status.Should().Be("Gepland");
                });
        }

        protected override BuildingUnitV2Projections CreateProjection() => new BuildingUnitV2Projections(WKBReaderFactory.Create());
    }
}
