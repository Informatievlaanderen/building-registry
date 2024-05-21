namespace BuildingRegistry.Tests.ProjectionTests.Integration.BuildingUnit
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Extensions;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.IO;
    using Projections.Integration.BuildingUnit.LatestItem;
    using Projections.Integration.Converters;
    using Projections.Integration.Infrastructure;
    using Tests.Legacy.Autofixture;
    using Xunit;

    public class BuildingUnitLatestItemProjectionsTests : IntegrationProjectionTest<BuildingUnitLatestItemProjections>
    {
        private const string BuildingNamespace = "https://data.vlaanderen.be/id/gebouw";
        private const string BuildingUnitNamespace = "https://data.vlaanderen.be/id/gebouweenheid";

        private readonly Fixture _fixture;
        private readonly WKBReader _wkbReader = WKBReaderFactory.Create();

        public BuildingUnitLatestItemProjectionsTests()
        {
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
                    var buildingUnits = context.BuildingUnitLatestItems
                        .Where(x => x.BuildingPersistentLocalId == buildingWasMigrated.BuildingPersistentLocalId)
                        .ToList();

                    foreach (var buildingUnit in buildingWasMigrated.BuildingUnits)
                    {
                        var buildingUnitLatestItem = buildingUnits
                            .Single(x => x.BuildingUnitPersistentLocalId == buildingUnit.BuildingUnitPersistentLocalId);

                        buildingUnitLatestItem.BuildingPersistentLocalId.Should().Be(buildingWasMigrated.BuildingPersistentLocalId);
                        buildingUnitLatestItem.OsloStatus.Should().Be(BuildingUnitStatus.Parse(buildingUnit.Status).Map());
                        buildingUnitLatestItem.Status.Should().Be(buildingUnit.Status);
                        buildingUnitLatestItem.OsloFunction.Should().Be(BuildingUnitFunction.Parse(buildingUnit.Function).Map());
                        buildingUnitLatestItem.Function.Should().Be(buildingUnit.Function);
                        buildingUnitLatestItem.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod).Map());
                        buildingUnitLatestItem.GeometryMethod.Should().Be(buildingUnit.GeometryMethod);
                        buildingUnitLatestItem.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingUnit.ExtendedWkbGeometry.ToByteArray()));
                        buildingUnitLatestItem.HasDeviation.Should().BeFalse();
                        buildingUnitLatestItem.IsRemoved.Should().Be(buildingUnit.IsRemoved);
                        buildingUnitLatestItem.Namespace.Should().Be(BuildingUnitNamespace);
                        buildingUnitLatestItem.Puri.Should().Be($"{BuildingUnitNamespace}/{buildingUnitLatestItem.BuildingUnitPersistentLocalId}");
                        buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingWasMigrated.Provenance.Timestamp);

                        var buildingUnitAddresses = context.BuildingUnitAddresses
                            .Where(x => x.BuildingUnitPersistentLocalId == buildingUnitLatestItem.BuildingUnitPersistentLocalId)
                            .ToList();

                        buildingUnitAddresses.Should().HaveCount(buildingUnit.AddressPersistentLocalIds.Count);
                        foreach (var addressPersistentLocalId in buildingUnit.AddressPersistentLocalIds)
                        {
                            buildingUnitAddresses.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId)
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
            var buildingOutlineWasChanged = _fixture.Create<BuildingOutlineWasChanged>();

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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    buildingUnitLatestItem.GeometryMethod.Should().Be("DerivedFromObject");
                    buildingUnitLatestItem.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingOutlineWasChanged.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingOutlineWasChanged.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingWasMeasured()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingWasMeasured = _fixture.Create<BuildingWasMeasured>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingWasMeasuredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMeasured.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingWasMeasured>(new Envelope(buildingWasMeasured, buildingWasMeasuredMetadata)))
                .Then(async context =>
                {
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    buildingUnitLatestItem.GeometryMethod.Should().Be("DerivedFromObject");
                    buildingUnitLatestItem.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingWasMeasured.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingWasMeasured.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementWasCorrected()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingMeasurementWasCorrected = _fixture.Create<BuildingMeasurementWasCorrected>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingMeasurementWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingMeasurementWasCorrected.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingMeasurementWasCorrected>(new Envelope(buildingMeasurementWasCorrected, buildingMeasurementWasCorrectedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    buildingUnitLatestItem.GeometryMethod.Should().Be("DerivedFromObject");
                    buildingUnitLatestItem.Geometry.Should()
                        .BeEquivalentTo(_wkbReader.Read(buildingMeasurementWasCorrected.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingMeasurementWasCorrected.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementWasChanged()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingMeasurementWasChanged = _fixture.Create<BuildingMeasurementWasChanged>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingMeasurementWasChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingMeasurementWasChanged.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingMeasurementWasChanged>(new Envelope(buildingMeasurementWasChanged, buildingMeasurementWasChangedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitLatestItem = await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasPlannedV2
                        .BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.DerivedFromObject.Map());
                    buildingUnitLatestItem.GeometryMethod.Should().Be("DerivedFromObject");
                    buildingUnitLatestItem.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingMeasurementWasChanged.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingMeasurementWasChanged.Provenance.Timestamp);
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
                    var buildingUnitLatestItem = await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.BuildingPersistentLocalId.Should().Be(buildingUnitWasPlannedV2.BuildingPersistentLocalId);
                    buildingUnitLatestItem.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitWasPlannedV2.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitLatestItem.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitWasPlannedV2.GeometryMethod).Map());
                    buildingUnitLatestItem.GeometryMethod.Should().Be(buildingUnitWasPlannedV2.GeometryMethod);
                    buildingUnitLatestItem.OsloFunction.Should().Be(BuildingUnitFunction.Parse(buildingUnitWasPlannedV2.Function).Map());
                    buildingUnitLatestItem.Function.Should().Be(buildingUnitWasPlannedV2.Function);
                    buildingUnitLatestItem.OsloStatus.Should().Be(BuildingUnitStatus.Planned.Map());
                    buildingUnitLatestItem.Status.Should().Be("Planned");
                    buildingUnitLatestItem.HasDeviation.Should().Be(buildingUnitWasPlannedV2.HasDeviation);
                    buildingUnitLatestItem.IsRemoved.Should().BeFalse();
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasPlannedV2.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasRealizedV2.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloStatus.Should().Be(BuildingUnitStatus.Realized.Map());
                    buildingUnitLatestItem.Status.Should().Be("Realized");
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasRealizedV2.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasRealized.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloStatus.Should().Be(BuildingUnitStatus.Realized.Map());
                    buildingUnitLatestItem.Status.Should().Be("Realized");
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasRealized.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasCorrectedFromRealizedToPlanned.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloStatus.Should().Be(BuildingUnitStatus.Planned.Map());
                    buildingUnitLatestItem.Status.Should().Be("Planned");
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasCorrectedFromRealizedToPlanned.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasCorrectedToPlanned.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloStatus.Should().Be(BuildingUnitStatus.Planned.Map());
                    buildingUnitLatestItem.Status.Should().Be("Planned");
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToPlanned.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasNotRealizedV2.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloStatus.Should().Be(BuildingUnitStatus.NotRealized.Map());
                    buildingUnitLatestItem.Status.Should().Be("NotRealized");
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasNotRealizedV2.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasNotRealized.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloStatus.Should().Be(BuildingUnitStatus.NotRealized.Map());
                    buildingUnitLatestItem.Status.Should().Be("NotRealized");
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasNotRealized.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasCorrectedToPlanned.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloStatus.Should().Be(BuildingUnitStatus.Planned.Map());
                    buildingUnitLatestItem.Status.Should().Be("Planned");
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToPlanned.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasRetiredV2.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloStatus.Should().Be(BuildingUnitStatus.Retired.Map());
                    buildingUnitLatestItem.Status.Should().Be("Retired");
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasRetiredV2.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasCorrectedFromRetiredToRealized()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitWasRetiredV2 = _fixture.Create<BuildingUnitWasRetiredV2>();
            var buildingUnitWasCorrectedToRealized = _fixture.Create<BuildingUnitWasCorrectedFromRetiredToRealized>();

            var position = _fixture.Create<long>();

            var buildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position }
            };
            var buildingUnitWasRetiredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasRetiredV2.GetHash() },
                { Envelope.PositionMetadataKey, position + 1 }
            };
            var buildingUnitWasCorrectedToRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasCorrectedToRealized.GetHash() },
                { Envelope.PositionMetadataKey, position + 2 }
            };

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasRetiredV2>(new Envelope(buildingUnitWasRetiredV2, buildingUnitWasRetiredMetadata)),
                    new Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>(
                        new Envelope(buildingUnitWasCorrectedToRealized, buildingUnitWasCorrectedToRealizedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasCorrectedToRealized.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloStatus.Should().Be(BuildingUnitStatus.Realized.Map());
                    buildingUnitLatestItem.Status.Should().Be("Realized");
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasCorrectedToRealized.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitPositionWasCorrected.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.BuildingPersistentLocalId.Should().Be(buildingUnitPositionWasCorrected.BuildingPersistentLocalId);
                    buildingUnitLatestItem.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitPositionWasCorrected.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitLatestItem.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitPositionWasCorrected.GeometryMethod).Map());
                    buildingUnitLatestItem.GeometryMethod.Should().Be(buildingUnitPositionWasCorrected.GeometryMethod);
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitPositionWasCorrected.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasRemovedV2.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.IsRemoved.Should().BeTrue();
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasRemovedV2.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasRemoved.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.IsRemoved.Should().BeTrue();
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasRemoved.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitRemovalWasCorrected.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloStatus.Should().Be(BuildingUnitStatus.Parse(buildingUnitRemovalWasCorrected.BuildingUnitStatus).Map());
                    buildingUnitLatestItem.Status.Should().Be(buildingUnitRemovalWasCorrected.BuildingUnitStatus);
                    buildingUnitLatestItem.OsloFunction.Should().Be(BuildingUnitFunction.Parse(buildingUnitRemovalWasCorrected.Function).Map());
                    buildingUnitLatestItem.Function.Should().Be(buildingUnitRemovalWasCorrected.Function);
                    buildingUnitLatestItem.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitRemovalWasCorrected.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitLatestItem.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitRemovalWasCorrected.GeometryMethod).Map());
                    buildingUnitLatestItem.GeometryMethod.Should().Be(buildingUnitRemovalWasCorrected.GeometryMethod);
                    buildingUnitLatestItem.HasDeviation.Should().Be(buildingUnitRemovalWasCorrected.HasDeviation);
                    buildingUnitLatestItem.IsRemoved.Should().BeFalse();
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitRemovalWasCorrected.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasRegularized.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.HasDeviation.Should().BeFalse();
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasRegularized.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitRegularizationWasCorrected.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.HasDeviation.Should().BeTrue();
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitRegularizationWasCorrected.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasDeregulated.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.HasDeviation.Should().BeTrue();
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasDeregulated.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitDeregulationWasCorrected.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.HasDeviation.Should().BeFalse();
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitDeregulationWasCorrected.Provenance.Timestamp);
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
                    var buildingUnitLatestItem = await context.BuildingUnitLatestItems.FindAsync(commonBuildingUnitWasAddedV2
                        .BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.BuildingPersistentLocalId.Should().Be(commonBuildingUnitWasAddedV2.BuildingPersistentLocalId);
                    buildingUnitLatestItem.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(commonBuildingUnitWasAddedV2.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitLatestItem.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(commonBuildingUnitWasAddedV2.GeometryMethod).Map());
                    buildingUnitLatestItem.GeometryMethod.Should().Be(commonBuildingUnitWasAddedV2.GeometryMethod);
                    buildingUnitLatestItem.OsloFunction.Should().Be(BuildingUnitFunction.Common.Map());
                    buildingUnitLatestItem.Function.Should().Be(BuildingUnitFunction.Common.Function);
                    buildingUnitLatestItem.OsloStatus.Should().Be(BuildingUnitStatus.Parse(commonBuildingUnitWasAddedV2.BuildingUnitStatus).Map());
                    buildingUnitLatestItem.Status.Should().Be(commonBuildingUnitWasAddedV2.BuildingUnitStatus);
                    buildingUnitLatestItem.HasDeviation.Should().Be(commonBuildingUnitWasAddedV2.HasDeviation);
                    buildingUnitLatestItem.IsRemoved.Should().BeFalse();
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(commonBuildingUnitWasAddedV2.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitAddressWasAttachedV2.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.VersionTimestamp.Should().Be(buildingUnitAddressWasAttachedV2.Provenance.Timestamp);

                    var buildingUnitAddresses = context.BuildingUnitAddresses
                        .Where(x => x.BuildingUnitPersistentLocalId == buildingUnitLatestItem.BuildingUnitPersistentLocalId)
                        .ToList();
                    buildingUnitAddresses.Should().HaveCount(1);
                    buildingUnitAddresses.Single().AddressPersistentLocalId.Should().Be(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitAddressWasDetachedV2.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.VersionTimestamp.Should().Be(buildingUnitAddressWasDetachedV2.Provenance.Timestamp);

                    var buildingUnitAddresses = context.BuildingUnitAddresses
                        .Where(x => x.BuildingUnitPersistentLocalId == buildingUnitLatestItem.BuildingUnitPersistentLocalId)
                        .ToList();
                    buildingUnitAddresses.Should().BeEmpty();
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.VersionTimestamp.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    var buildingUnitAddresses = context.BuildingUnitAddresses
                        .Where(x => x.BuildingUnitPersistentLocalId == buildingUnitLatestItem.BuildingUnitPersistentLocalId)
                        .ToList();
                    buildingUnitAddresses.Should().BeEmpty();
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.VersionTimestamp.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    var buildingUnitAddresses = context.BuildingUnitAddresses
                        .Where(x => x.BuildingUnitPersistentLocalId == buildingUnitLatestItem.BuildingUnitPersistentLocalId)
                        .ToList();
                    buildingUnitAddresses.Should().BeEmpty();
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitAddressWasDetached.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.VersionTimestamp.Should().Be(buildingUnitAddressWasDetached.Provenance.Timestamp);

                    var buildingUnitAddresses = context.BuildingUnitAddresses
                        .Where(x => x.BuildingUnitPersistentLocalId == buildingUnitLatestItem.BuildingUnitPersistentLocalId)
                        .ToList();
                    buildingUnitAddresses.Should().BeEmpty();
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.VersionTimestamp.Should().Be(buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.Provenance.Timestamp);

                    var newAddress = context.BuildingUnitAddresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.NewAddressPersistentLocalId);
                    newAddress.Should().NotBeNull();

                    var oldAddress = context.BuildingUnitAddresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == buildingUnitAddressWasReplacedBecauseAddressWasReaddressed.PreviousAddressPersistentLocalId);
                    oldAddress.Should().BeNull();
                });
        }

        [Fact]
        public async Task WhenBuildingBuildingUnitsAddressesWereReaddressed()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var position = _fixture.Create<long>();
            var buildingUnitPersistentLocalId = _fixture.Create<BuildingUnitPersistentLocalId>();
            var sourceAddressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var destinationAddressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();

            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(new BuildingUnitBuilder(_fixture)
                    .WithAddress(sourceAddressPersistentLocalId)
                    .Build()
                ).Build();

            var buildingBuildingUnitsAddressesWereReaddressed = new BuildingBuildingUnitsAddressesWereReaddressed(
                _fixture.Create<BuildingPersistentLocalId>(),
                [
                    new BuildingUnitAddressesWereReaddressed(
                        _fixture.Create<BuildingUnitPersistentLocalId>(),
                        [new AddressPersistentLocalId(destinationAddressPersistentLocalId)],
                        [new AddressPersistentLocalId(sourceAddressPersistentLocalId)]
                    )
                ],
                []);
            ((ISetProvenance)buildingBuildingUnitsAddressesWereReaddressed).SetProvenance(_fixture.Create<Provenance>());

            var buildingWasMigratedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingBuildingUnitsAddressesWereReaddressedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingBuildingUnitsAddressesWereReaddressed.GetHash() },
                { Envelope.PositionMetadataKey, ++position }
            };
            await Sut
                .Given(
                    new Envelope<BuildingWasMigrated>(new Envelope(buildingWasMigrated, buildingWasMigratedMetadata)),
                    new Envelope<BuildingBuildingUnitsAddressesWereReaddressed>(
                        new Envelope(
                            buildingBuildingUnitsAddressesWereReaddressed,
                            buildingBuildingUnitsAddressesWereReaddressedMetadata)))
                .Then(async context =>
                {
                    var buildingUnitLatestItem = await context.BuildingUnitLatestItems.FindAsync((int)buildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.VersionTimestamp.Should().Be(buildingBuildingUnitsAddressesWereReaddressed.Provenance.Timestamp);

                    var destinationAddress = context.BuildingUnitAddresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == destinationAddressPersistentLocalId);
                    destinationAddress.Should().NotBeNull();

                    var sourceAddress = context.BuildingUnitAddresses.SingleOrDefault(x =>
                        x.AddressPersistentLocalId == sourceAddressPersistentLocalId);
                    sourceAddress.Should().BeNull();
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasRetired.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloStatus.Should().Be(BuildingUnitStatus.Retired.Map());
                    buildingUnitLatestItem.Status.Should().Be("Retired");
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasRetired.Provenance.Timestamp);
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
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasNotRealized.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.OsloStatus.Should().Be(BuildingUnitStatus.NotRealized.Map());
                    buildingUnitLatestItem.Status.Should().Be("NotRealized");
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasNotRealized.Provenance.Timestamp);
                });
        }

        [Fact]
        public async Task WhenBuildingUnitWasMovedIntoBuilding()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingUnitAddressWasAttachedV2 = _fixture.Create<BuildingUnitAddressWasAttachedV2>();
            var buildingUnitWasMovedIntoBuilding = _fixture.Create<BuildingUnitWasMovedIntoBuilding>()
                .WithAddressPersistentLocalIds(new[] { new AddressPersistentLocalId(buildingUnitAddressWasAttachedV2.AddressPersistentLocalId) });
            
            var position = _fixture.Create<long>();

            await Sut
                .Given(
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                        { Envelope.PositionMetadataKey, position }
                    })),
                    new Envelope<BuildingUnitAddressWasAttachedV2>(new Envelope(buildingUnitAddressWasAttachedV2, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingUnitAddressWasAttachedV2.GetHash() },
                        { Envelope.PositionMetadataKey, position + 1 }
                    })),
                    new Envelope<BuildingUnitWasMovedIntoBuilding>(new Envelope(buildingUnitWasMovedIntoBuilding, new Dictionary<string, object>
                    {
                        { AddEventHashPipe.HashMetadataKey, buildingUnitWasMovedIntoBuilding.GetHash() },
                        { Envelope.PositionMetadataKey, position + 2 }
                    })))
                .Then(async context =>
                {
                    var buildingUnitLatestItem =
                        await context.BuildingUnitLatestItems.FindAsync(buildingUnitWasMovedIntoBuilding.BuildingUnitPersistentLocalId);
                    buildingUnitLatestItem.Should().NotBeNull();

                    buildingUnitLatestItem!.BuildingPersistentLocalId.Should().Be(buildingUnitWasMovedIntoBuilding.BuildingPersistentLocalId);
                    buildingUnitLatestItem.OsloStatus.Should().Be(
                        BuildingUnitStatus.Parse(buildingUnitWasMovedIntoBuilding.BuildingUnitStatus).Map());
                    buildingUnitLatestItem.Status.Should().Be(buildingUnitWasMovedIntoBuilding.BuildingUnitStatus);
                    buildingUnitLatestItem.OsloFunction.Should().Be(
                        BuildingUnitFunction.Parse(buildingUnitWasMovedIntoBuilding.Function).Map());
                    buildingUnitLatestItem.Function.Should().Be(buildingUnitWasMovedIntoBuilding.Function);
                    buildingUnitLatestItem.OsloGeometryMethod.Should().Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnitWasMovedIntoBuilding.GeometryMethod).Map());
                    buildingUnitLatestItem.GeometryMethod.Should().Be(buildingUnitWasMovedIntoBuilding.GeometryMethod);
                    buildingUnitLatestItem.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingUnitWasMovedIntoBuilding.ExtendedWkbGeometry.ToByteArray()));
                    buildingUnitLatestItem.HasDeviation.Should().BeFalse();
                    buildingUnitLatestItem.VersionTimestamp.Should().Be(buildingUnitWasMovedIntoBuilding.Provenance.Timestamp);
                    
                    var buildingUnitAddresses = context.BuildingUnitAddresses
                        .Where(x => x.BuildingUnitPersistentLocalId == buildingUnitLatestItem.BuildingUnitPersistentLocalId)
                        .ToList();

                    buildingUnitAddresses.Should().HaveCount(buildingUnitWasMovedIntoBuilding.AddressPersistentLocalIds.Count);
                    foreach (var addressPersistentLocalId in buildingUnitWasMovedIntoBuilding.AddressPersistentLocalIds)
                    {
                        buildingUnitAddresses.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId)
                            .Should().NotBeNull();
                    }
                });
        }

        protected override BuildingUnitLatestItemProjections CreateProjection() =>
            new BuildingUnitLatestItemProjections(new OptionsWrapper<IntegrationOptions>(new IntegrationOptions
            {
                BuildingNamespace = BuildingNamespace,
                BuildingUnitNamespace = BuildingUnitNamespace
            }));
    }
}
