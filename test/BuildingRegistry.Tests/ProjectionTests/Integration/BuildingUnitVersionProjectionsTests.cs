namespace BuildingRegistry.Tests.ProjectionTests.Integration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using Moq;
    using NetTopologySuite.IO;
    using Projections.Integration;
    using Projections.Integration.BuildingUnit.Version;
    using Projections.Integration.Converters;
    using Projections.Integration.Infrastructure;
    using Tests.Legacy.Autofixture;
    using Xunit;

    public class BuildingUnitVersionProjectionsTests : IntegrationProjectionTest<BuildingUnitVersionProjections>
    {
        private const string BuildingNamespace = "https://data.vlaanderen.be/id/gebouw";
        private const string BuildingUnitNamespace = "https://data.vlaanderen.be/id/gebouweenheid";

        private readonly Fixture _fixture;
        private readonly WKBReader _wkbReader = WKBReaderFactory.Create();
        private readonly Mock<IPersistentLocalIdFinder> _persistentLocalIdFinder;

        public BuildingUnitVersionProjectionsTests()
        {
            _persistentLocalIdFinder = new Mock<IPersistentLocalIdFinder>();

            _fixture = new Fixture();
            _fixture.Customizations.Add(new WithUniqueInteger());
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithBuildingStatus());
            _fixture.Customize(new WithBuildingGeometryMethod());
            _fixture.Customize(new WithValidExtendedWkbPolygon());
            _fixture.Customize(new WithBuildingUnitStatus());
            _fixture.Customize(new WithBuildingUnitFunction());
            _fixture.Customize(new WithBuildingUnitPositionGeometryMethod());
        }

        [Theory]
        [InlineData("Planned")]
        [InlineData("UnderConstruction")]
        [InlineData("Realized")]
        [InlineData("Retired")]
        [InlineData("NotRealized")]
        public async Task WhenBuildingWasMigrated(string buildingStatus)
        {
            _fixture.Register(() => BuildingStatus.Parse(buildingStatus));

            var buildingWasMigrated = _fixture.Create<BuildingWasMigrated>();

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<BuildingWasMigrated>(new Envelope(buildingWasMigrated, metadata)))
                .Then(context =>
                {
                    var buildingUnits = context.BuildingUnitVersions
                        .Where(x => x.BuildingPersistentLocalId == buildingWasMigrated.BuildingPersistentLocalId)
                        .ToList();

                    foreach (var buildingUnit in buildingWasMigrated.BuildingUnits)
                    {
                        var buildingUnitVersion = buildingUnits
                            .Single(x => x.BuildingUnitPersistentLocalId == buildingUnit.BuildingUnitPersistentLocalId);

                        buildingUnitVersion.BuildingPersistentLocalId.Should().Be(buildingWasMigrated.BuildingPersistentLocalId);
                        buildingUnitVersion.Status.Should().Be(BuildingUnitStatus.Parse(buildingUnit.Status).Map());
                        buildingUnitVersion.Function.Should().Be(BuildingUnitFunction.Parse(buildingUnit.Function).Map());
                        buildingUnitVersion.GeometryMethod.Should()
                            .Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod).Map());
                        buildingUnitVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingUnit.ExtendedWkbGeometry.ToByteArray()));
                        buildingUnitVersion.HasDeviation.Should().BeFalse();
                        buildingUnitVersion.IsRemoved.Should().Be(buildingUnit.IsRemoved);
                        buildingUnitVersion.Namespace.Should().Be(BuildingUnitNamespace);
                        buildingUnitVersion.PuriId.Should().Be($"{BuildingUnitNamespace}/{buildingUnitVersion.BuildingUnitPersistentLocalId}");
                        buildingUnitVersion.VersionTimestamp.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                        buildingUnitVersion.CreatedOnTimestamp.Should().Be(buildingWasMigrated.Provenance.Timestamp);

                        buildingUnitVersion.Addresses.Should().HaveCount(3);
                        foreach (var addressPersistentLocalId in buildingUnit.AddressPersistentLocalIds)
                        {
                            buildingUnitVersion.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId)
                                .Should().NotBeNull();
                        }
                    }

                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task WhenBuildingOutlineWasChanged()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingOutlineWasChanged = new BuildingOutlineWasChanged(
                _fixture.Create<BuildingPersistentLocalId>(),
                new []{ _fixture.Create<BuildingUnitPersistentLocalId>() },
                _fixture.Create<ExtendedWkbGeometry>(),
                _fixture.Create<ExtendedWkbGeometry>());
            ((ISetProvenance)buildingOutlineWasChanged).SetProvenance(_fixture.Create<Provenance>());

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingOutLineWasChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingOutlineWasChanged.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingOutlineWasChanged>(new Envelope(buildingOutlineWasChanged, buildingOutLineWasChangedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    buildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingOutlineWasChanged.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingOutlineWasChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasMeasured()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());

            var firstBuildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var secondBuildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingWasMeasured = new BuildingWasMeasured(
                _fixture.Create<BuildingPersistentLocalId>(),
                new []{ new BuildingUnitPersistentLocalId(firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId) },
                new []{ new BuildingUnitPersistentLocalId(secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId) },
                _fixture.Create<ExtendedWkbGeometry>(),
                _fixture.Create<ExtendedWkbGeometry>());
            ((ISetProvenance)buildingWasMeasured).SetProvenance(_fixture.Create<Provenance>());

            var position = _fixture.Create<long>();

            var firstBuildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstBuildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var secondBuildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstBuildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingWasMeasuredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMeasured.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(firstBuildingUnitWasPlannedV2, firstBuildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(secondBuildingUnitWasPlannedV2, secondBuildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingWasMeasured>(new Envelope(buildingWasMeasured, buildingWasMeasuredMetadata)))
                .Then(async context =>
                {
                    var firstBuildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    firstBuildingUnitVersion.Should().NotBeNull();

                    firstBuildingUnitVersion!.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    firstBuildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingWasMeasured.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    firstBuildingUnitVersion.VersionTimestamp.Should().Be(buildingWasMeasured.Provenance.Timestamp);

                    var secondBuildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    secondBuildingUnitVersion.Should().NotBeNull();

                    secondBuildingUnitVersion!.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    secondBuildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingWasMeasured.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    secondBuildingUnitVersion.VersionTimestamp.Should().Be(buildingWasMeasured.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());

            var firstBuildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var secondBuildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingMeasurementWasCorrected = new BuildingMeasurementWasCorrected(
                _fixture.Create<BuildingPersistentLocalId>(),
                new []{ new BuildingUnitPersistentLocalId(firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId) },
                new []{ new BuildingUnitPersistentLocalId(secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId) },
                _fixture.Create<ExtendedWkbGeometry>(),
                _fixture.Create<ExtendedWkbGeometry>());
            ((ISetProvenance)buildingMeasurementWasCorrected).SetProvenance(_fixture.Create<Provenance>());

            var position = _fixture.Create<long>();

            var firstBuildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstBuildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var secondBuildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstBuildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingMeasurementWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingMeasurementWasCorrected.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(firstBuildingUnitWasPlannedV2, firstBuildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(secondBuildingUnitWasPlannedV2, secondBuildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingMeasurementWasCorrected>(new Envelope(buildingMeasurementWasCorrected, buildingMeasurementWasCorrectedMetadata)))
                .Then(async context =>
                {
                    var firstBuildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    firstBuildingUnitVersion.Should().NotBeNull();

                    firstBuildingUnitVersion!.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    firstBuildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingMeasurementWasCorrected.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    firstBuildingUnitVersion.VersionTimestamp.Should().Be(buildingMeasurementWasCorrected.Provenance.Timestamp);

                    var secondBuildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    secondBuildingUnitVersion.Should().NotBeNull();

                    secondBuildingUnitVersion!.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    secondBuildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingMeasurementWasCorrected.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    secondBuildingUnitVersion.VersionTimestamp.Should().Be(buildingMeasurementWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementWasChanged()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());

            var firstBuildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var secondBuildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingMeasurementWasChanged = new BuildingMeasurementWasChanged(
                _fixture.Create<BuildingPersistentLocalId>(),
                new []{ new BuildingUnitPersistentLocalId(firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId) },
                new []{ new BuildingUnitPersistentLocalId(secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId) },
                _fixture.Create<ExtendedWkbGeometry>(),
                _fixture.Create<ExtendedWkbGeometry>());
            ((ISetProvenance)buildingMeasurementWasChanged).SetProvenance(_fixture.Create<Provenance>());

            var position = _fixture.Create<long>();

            var firstBuildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstBuildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var secondBuildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstBuildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingMeasurementWasChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingMeasurementWasChanged.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(firstBuildingUnitWasPlannedV2, firstBuildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(secondBuildingUnitWasPlannedV2, secondBuildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingMeasurementWasChanged>(new Envelope(buildingMeasurementWasChanged, buildingMeasurementWasChangedMetadata)))
                .Then(async context =>
                {
                    var firstBuildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    firstBuildingUnitVersion.Should().NotBeNull();

                    firstBuildingUnitVersion!.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    firstBuildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingMeasurementWasChanged.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    firstBuildingUnitVersion.VersionTimestamp.Should().Be(buildingMeasurementWasChanged.Provenance.Timestamp);

                    var secondBuildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    secondBuildingUnitVersion.Should().NotBeNull();

                    secondBuildingUnitVersion!.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    secondBuildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingMeasurementWasChanged.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    secondBuildingUnitVersion.VersionTimestamp.Should().Be(buildingMeasurementWasChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasPlannedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, metadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position, buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.BuildingPersistentLocalId.Should().Be(buildingUnitWasPlannedV2.BuildingPersistentLocalId);
                    buildingUnitVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingUnitWasPlannedV2.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.GeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitWasPlannedV2.GeometryMethod).Map());
                    buildingUnitVersion.Function.Should().Be(BuildingUnitFunction.Parse(buildingUnitWasPlannedV2.Function).Map());
                    buildingUnitVersion.Status.Should().Be(BuildingUnitStatus.Planned.Map());
                    buildingUnitVersion.HasDeviation.Should().Be(buildingUnitWasPlannedV2.HasDeviation);
                    buildingUnitVersion.IsRemoved.Should().BeFalse();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                    buildingUnitVersion.CreatedOnTimestamp.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
                    buildingUnitVersion.Namespace.Should().Be(BuildingUnitNamespace);
                    buildingUnitVersion.PuriId.Should().Be($"{BuildingUnitNamespace}/{buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId}");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRealizedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(buildingUnitWasRealizedV2, buildingUnitWasRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitWasRealizedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Realized.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRealizedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRealizedBecauseBuildingWasRealized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealized = _fixture.Create<BuildingUnitWasRealizedBecauseBuildingWasRealized>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealized.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>(new Envelope(buildingUnitWasRealized, buildingUnitWasRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitWasRealized.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Realized.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRealizedToPlanned()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();
            var buildingUnitWasCorrectedFromRealizedToPlanned = _fixture.Create<BuildingUnitWasCorrectedFromRealizedToPlanned>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasCorrectedFromRealizedToPlanned.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(buildingUnitWasRealizedV2, buildingUnitWasRealizedMetadata)),
                    new Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>(
                        new Envelope(buildingUnitWasCorrectedFromRealizedToPlanned, buildingUnitWasCorrectedToPlannedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, buildingUnitWasCorrectedFromRealizedToPlanned.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Planned.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasCorrectedFromRealizedToPlanned.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();
            var buildingUnitWasCorrectedToPlanned = _fixture.Create<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasCorrectedToPlanned.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(buildingUnitWasRealizedV2, buildingUnitWasRealizedMetadata)),
                    new Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>(
                        new Envelope(buildingUnitWasCorrectedToPlanned, buildingUnitWasCorrectedToPlannedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, buildingUnitWasCorrectedToPlanned.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Planned.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToPlanned.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasNotRealizedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasNotRealizedV2 = _fixture.Create<BuildingUnitWasNotRealizedV2>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasNotRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasNotRealizedV2>(new Envelope(buildingUnitWasNotRealizedV2, buildingUnitWasNotRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitWasNotRealizedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.NotRealized.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasNotRealizedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasNotRealizedBecauseBuildingWasNotRealized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasNotRealized = _fixture.Create<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasNotRealized.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>(
                        new Envelope(buildingUnitWasNotRealized, buildingUnitWasNotRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitWasNotRealized.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.NotRealized.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasNotRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromNotRealizedToPlanned()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasNotRealizedV2 = _fixture.Create<BuildingUnitWasNotRealizedV2>();
            var buildingUnitWasCorrectedToPlanned = _fixture.Create<BuildingUnitWasCorrectedFromNotRealizedToPlanned>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasNotRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasCorrectedToPlanned.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasNotRealizedV2>(new Envelope(buildingUnitWasNotRealizedV2, buildingUnitWasNotRealizedMetadata)),
                    new Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>(
                        new Envelope(buildingUnitWasCorrectedToPlanned, buildingUnitWasCorrectedToPlannedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, buildingUnitWasCorrectedToPlanned.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Planned.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToPlanned.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRetiredV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();
            var buildingUnitWasRetiredV2 = _fixture.Create<BuildingUnitWasRetiredV2>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitWasRetiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRetiredV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(buildingUnitWasRealizedV2, buildingUnitWasRealizedMetadata)),
                    new Envelope<BuildingUnitWasRetiredV2>(new Envelope(buildingUnitWasRetiredV2, buildingUnitWasRetiredMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, buildingUnitWasRetiredV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Retired.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRetiredV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRetiredToRealized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRetiredV2 = _fixture.Create<BuildingUnitWasRetiredV2>();
            var buildingUnitWasRealizedV2 = _fixture.Create<BuildingUnitWasRealizedV2>();
            var buildingUnitWasCorrectedToRealized = _fixture.Create<BuildingUnitWasCorrectedFromRetiredToRealized>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitWasRetiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRetiredV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };
            var buildingUnitWasCorrectedToRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasCorrectedToRealized.GetHash() },
                { Envelope.PositionMetadataKey, position + 3 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRealizedV2>(new Envelope(buildingUnitWasRealizedV2, buildingUnitWasRealizedMetadata)),
                    new Envelope<BuildingUnitWasRetiredV2>(new Envelope(buildingUnitWasRetiredV2, buildingUnitWasRetiredMetadata)),
                    new Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>(
                        new Envelope(buildingUnitWasCorrectedToRealized, buildingUnitWasCorrectedToRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 3, buildingUnitWasCorrectedToRealized.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Realized.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitPositionWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitPositionWasCorrected = _fixture.Create<BuildingUnitPositionWasCorrected>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitPositionWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitPositionWasCorrected.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitPositionWasCorrected>(new Envelope(buildingUnitPositionWasCorrected, buildingUnitPositionWasCorrectedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitPositionWasCorrected.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.BuildingPersistentLocalId.Should().Be(buildingUnitPositionWasCorrected.BuildingPersistentLocalId);
                    buildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitPositionWasCorrected.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.GeometryMethod.Should()
                        .Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitPositionWasCorrected.GeometryMethod).Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitPositionWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRemovedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRemovedV2 = _fixture.Create<BuildingUnitWasRemovedV2>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRemovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRemovedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRemovedV2>(new Envelope(buildingUnitWasRemovedV2, buildingUnitWasRemovedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitWasRemovedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.IsRemoved.Should().BeTrue();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRemovedV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRemovedBecauseBuildingWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRemoved = _fixture.Create<BuildingUnitWasRemovedBecauseBuildingWasRemoved>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRemovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRemoved.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>(new Envelope(buildingUnitWasRemoved, buildingUnitWasRemovedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitWasRemoved.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.IsRemoved.Should().BeTrue();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRemoved.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitRemovalWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRemovedV2 = _fixture.Create<BuildingUnitWasRemovedV2>();
            var buildingUnitRemovalWasCorrected = _fixture.Create<BuildingUnitRemovalWasCorrected>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRemovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRemovedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitRemovalWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitRemovalWasCorrected.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRemovedV2>(new Envelope(buildingUnitWasRemovedV2, buildingUnitWasRemovedMetadata)),
                    new Envelope<BuildingUnitRemovalWasCorrected>(new Envelope(buildingUnitRemovalWasCorrected, buildingUnitRemovalWasCorrectedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, buildingUnitRemovalWasCorrected.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Parse(buildingUnitRemovalWasCorrected.BuildingUnitStatus).Map());
                    buildingUnitVersion.Function.Should().Be(BuildingUnitFunction.Parse(buildingUnitRemovalWasCorrected.Function).Map());
                    buildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitRemovalWasCorrected.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.GeometryMethod.Should()
                        .Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitRemovalWasCorrected.GeometryMethod).Map());
                    buildingUnitVersion.HasDeviation.Should().Be(buildingUnitRemovalWasCorrected.HasDeviation);
                    buildingUnitVersion.IsRemoved.Should().BeFalse();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitRemovalWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRegularized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>().WithDeviation(true);
            var buildingUnitWasRegularized = _fixture.Create<BuildingUnitWasRegularized>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRegularizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRegularized.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRegularized>(new Envelope(buildingUnitWasRegularized, buildingUnitWasRegularizedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitWasRegularized.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.HasDeviation.Should().BeFalse();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRegularized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitRegularizationWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>().WithDeviation(false);
            var buildingUnitRegularizationWasCorrected = _fixture.Create<BuildingUnitRegularizationWasCorrected>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitRegularizationWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitRegularizationWasCorrected.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitRegularizationWasCorrected>(
                        new Envelope(buildingUnitRegularizationWasCorrected, buildingUnitRegularizationWasCorrectedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitRegularizationWasCorrected.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.HasDeviation.Should().BeTrue();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitRegularizationWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasDeregulated()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>().WithDeviation(false);
            var buildingUnitWasDeregulated = _fixture.Create<BuildingUnitWasDeregulated>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasDeregulatedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasDeregulated.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasDeregulated>(new Envelope(buildingUnitWasDeregulated, buildingUnitWasDeregulatedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitWasDeregulated.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.HasDeviation.Should().BeTrue();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasDeregulated.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitDeregulationWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>().WithDeviation(true);
            var buildingUnitDeregulationWasCorrected = _fixture.Create<BuildingUnitDeregulationWasCorrected>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitDeregulationWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitDeregulationWasCorrected.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitDeregulationWasCorrected>(
                        new Envelope(buildingUnitDeregulationWasCorrected, buildingUnitDeregulationWasCorrectedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitDeregulationWasCorrected.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.HasDeviation.Should().BeFalse();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitDeregulationWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenCommonBuildingUnitWasAddedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var commonBuildingUnitWasAddedV2 = new CommonBuildingUnitWasAddedV2(
                _fixture.Create<BuildingPersistentLocalId>(),
                _fixture.Create<BuildingUnitPersistentLocalId>(),
                BuildingUnitStatus.Planned,
                BuildingUnitPositionGeometryMethod.DerivedFromObject,
                _fixture.Create<ExtendedWkbGeometry>(),
                false);
            ((ISetProvenance)commonBuildingUnitWasAddedV2).SetProvenance(_fixture.Create<Provenance>());

            var position = _fixture.Create<long>();

            var commonBuildingUnitWasAddedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, commonBuildingUnitWasAddedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };

            await Sut
                .Given(new Envelope<CommonBuildingUnitWasAddedV2>(new Envelope(commonBuildingUnitWasAddedV2, commonBuildingUnitWasAddedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion = await context.BuildingUnitVersions.FindAsync(position, commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.BuildingPersistentLocalId.Should().Be(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
                    buildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(commonBuildingUnitWasAddedV2.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.GeometryMethod.Should()
                        .Be(BuildingUnitPositionGeometryMethod.Parse(commonBuildingUnitWasAddedV2.GeometryMethod).Map());
                    buildingUnitVersion.Function.Should()
                        .Be(BuildingUnitFunction.Common.Map());
                    buildingUnitVersion.Status.Should().Be(
                        BuildingUnitStatus.Parse(commonBuildingUnitWasAddedV2.BuildingUnitStatus).Map());
                    buildingUnitVersion.HasDeviation.Should().Be(commonBuildingUnitWasAddedV2.HasDeviation);
                    buildingUnitVersion.IsRemoved.Should().BeFalse();
                    buildingUnitVersion.VersionTimestamp.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                    buildingUnitVersion.CreatedOnTimestamp.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
                    buildingUnitVersion.Namespace.Should().Be(BuildingUnitNamespace);
                    buildingUnitVersion.PuriId.Should().Be($"{BuildingUnitNamespace}/{commonBuildingUnitWasAddedV2.BuildingUnitPersistentLocalId}");
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasAttachedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttachedV2 = _fixture.Create<BuildingUnitAddressWasAttachedV2>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttachedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(new Envelope(buildingUnitAddressWasAttachedV2, buildingUnitAddressWasAttachedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasAttachedV2.Provenance.Timestamp);

                    buildingUnitVersion.Addresses.Should().HaveCount(1);
                    buildingUnitVersion.Addresses.Single().AddressPersistentLocalId.Should().Be(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedV2()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasDetachedV2 = _fixture.Create<BuildingUnitAddressWasDetachedV2>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttached.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitAddressWasDetachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasDetachedV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(new Envelope(buildingUnitAddressWasAttached, buildingUnitAddressWasAttachedMetadata)),
                    new Envelope<BuildingUnitAddressWasDetachedV2>(new Envelope(buildingUnitAddressWasDetachedV2, buildingUnitAddressWasDetachedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, buildingUnitAddressWasDetachedV2.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasDetachedV2.Provenance.Timestamp);

                    buildingUnitVersion.Addresses.Should().BeEmpty();
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedBecauseAddressWasRetired()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasDetached = _fixture.Create<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttached.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitAddressWasDetachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasDetached.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(
                        new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(
                        new Envelope(buildingUnitAddressWasAttached, buildingUnitAddressWasAttachedMetadata)),
                    new Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>(
                        new Envelope(buildingUnitAddressWasDetached, buildingUnitAddressWasDetachedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    buildingUnitVersion.Addresses.Should().BeEmpty();
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedBecauseAddressWasRejected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasDetached = _fixture.Create<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttached.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitAddressWasDetachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasDetached.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(
                        new Envelope(buildingUnitAddressWasAttached, buildingUnitAddressWasAttachedMetadata)),
                    new Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>(
                        new Envelope(buildingUnitAddressWasDetached, buildingUnitAddressWasDetachedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    buildingUnitVersion.Addresses.Should().BeEmpty();
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasDetachedBecauseAddressWasRemoved()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasDetached = _fixture.Create<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttached.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitAddressWasDetachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasDetached.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(
                        new Envelope(buildingUnitAddressWasAttached, buildingUnitAddressWasAttachedMetadata)),
                    new Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>(
                        new Envelope(buildingUnitAddressWasDetached, buildingUnitAddressWasDetachedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    buildingUnitVersion.Addresses.Should().BeEmpty();
                });
        }

        [Fact]
        public async Task WhenBuildingUnitAddressWasReplacedBecauseAddressWasReaddressed()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
            _fixture.Customize(new WithFixedAddressPersistentLocalId());

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttached = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitAddressWasReplacedBecauseAddressWasReaddressed = new BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                _fixture.Create<BuildingUnitPersistentLocalId>(),
                new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId),
                new AddressPersistentLocalId(buildingUnitAddressWasAttached.AddressPersistentLocalId + 1));
            ((ISetProvenance)buildingUnitAddressWasReplacedBecauseAddressWasReaddressed).SetProvenance(_fixture.Create<Provenance>());

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitAddressWasAttachedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttached.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitAddressWasReplacedBecauseAddressWasReaddressedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };
            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(new Envelope(buildingUnitAddressWasAttached, buildingUnitAddressWasAttachedMetadata)),
                    new Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>(
                        new Envelope(
                            buildingUnitAddressWasReplacedBecauseAddressWasReaddressed,
                            buildingUnitAddressWasReplacedBecauseAddressWasReaddressedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 2, buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.VersionTimestamp.Should().Be(buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.Provenance.Timestamp);

                    var newAddress = buildingUnitVersion.Addresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.NewAddressPersistentLocalId);
                    newAddress.Should().NotBeNull();

                    var oldAddress = buildingUnitVersion.Addresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.PreviousAddressPersistentLocalId);
                    oldAddress.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasRetiredBecauseBuildingWasDemolished()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRetired = _fixture.Create<BuildingUnitWasRetiredBecauseBuildingWasDemolished>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRetiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRetired.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>(new Envelope(buildingUnitWasRetired, buildingUnitWasRetiredMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitWasRetired.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.Retired.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasRetired.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasNotRealizedBecauseBuildingWasDemolished()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasNotRealized = _fixture.Create<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasNotRealized.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>(new Envelope(buildingUnitWasNotRealized, buildingUnitWasNotRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitWasNotRealized.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.Status.Should().Be(BuildingUnitStatus.NotRealized.Map());
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasNotRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasTransferred()
        {
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasTransferred = new BuildingUnitWasTransferredBuilder(_fixture)
                .WithBuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId)
                .WithSourceBuildingPersistentLocalId(buildingUnitWasPlannedV2.BuildingPersistentLocalId)
                .Build();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasTransferredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasTransferred.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasTransferred>(new Envelope(buildingUnitWasTransferred, buildingUnitWasTransferredMetadata)))
                .Then(async context =>
                {
                    var buildingUnitVersion =
                        await context.BuildingUnitVersions.FindAsync(position + 1, buildingUnitWasTransferred.BuildingUnitPersistentLocalId);
                    buildingUnitVersion.Should().NotBeNull();

                    buildingUnitVersion!.BuildingPersistentLocalId.Should().Be(buildingUnitWasTransferred.BuildingPersistentLocalId);
                    buildingUnitVersion.Status.Should().Be(
                        BuildingUnitStatus.Parse(buildingUnitWasTransferred.Status).Map());
                    buildingUnitVersion.Function.Should().Be(
                        BuildingUnitFunction.Parse(buildingUnitWasTransferred.Function).Map());
                    buildingUnitVersion.GeometryMethod.Should().Be(
                        BuildingUnitPositionGeometryMethod.Parse(buildingUnitWasTransferred.GeometryMethod).Map());
                    buildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitWasTransferred.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitVersion.HasDeviation.Should().BeFalse();
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingUnitWasTransferred.Provenance.Timestamp);

                    buildingUnitVersion.Addresses.Should().HaveCount(buildingUnitWasTransferred.AddressPersistentLocalIds.Count);
                    foreach (var addressPersistentLocalId in buildingUnitWasTransferred.AddressPersistentLocalIds)
                    {
                        buildingUnitVersion.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId)
                            .Should().NotBeNull();
                    }
                });
        }

        protected override BuildingUnitVersionProjections CreateProjection() =>
            new BuildingUnitVersionProjections(new OptionsWrapper<IntegrationOptions>(new IntegrationOptions
            {
                BuildingNamespace = BuildingNamespace,
                BuildingUnitNamespace = BuildingUnitNamespace,
            }),
            _persistentLocalIdFinder.Object);
    }
}
