// ReSharper disable EntityFramework.NPlusOne.IncompleteDataUsage
// ReSharper disable EntityFramework.NPlusOne.IncompleteDataQuery

namespace BuildingRegistry.Tests.ProjectionTests.Integration.Building
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
    using Moq;
    using NetTopologySuite.IO;
    using Projections.Integration;
    using Projections.Integration.Building.Version;
    using Projections.Integration.Converters;
    using Projections.Integration.Infrastructure;
    using Tests.Legacy.Autofixture;
    using Xunit;
    using BuildingUnit = BuildingRegistry.Building.Commands.BuildingUnit;
    using IAddresses = Projections.Integration.IAddresses;

    public partial class BuildingVersionProjectionsTests : IntegrationProjectionTest<BuildingVersionProjections>
    {
        private const string BuildingNamespace = "https://data.vlaanderen.be/id/gebouw";
        private const string BuildingUnitNamespace = "https://data.vlaanderen.be/id/gebouweenheid";

        private readonly Fixture _fixture;
        private readonly WKBReader _wkbReader = WKBReaderFactory.Create();
        private readonly Mock<IPersistentLocalIdFinder> _persistentLocalIdFinder;
        private readonly Mock<IAddresses> _addresses;

        public BuildingVersionProjectionsTests()
        {
            _persistentLocalIdFinder = new Mock<IPersistentLocalIdFinder>();
            _addresses = new Mock<IAddresses>();

            _fixture = new Fixture();
            _fixture.Customizations.Add(new WithUniqueInteger());
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
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
            var buildingWasMigrated = new BuildingWasMigratedBuilder(_fixture)
                .WithBuildingUnit(_fixture.Create<BuildingUnit>())
                .Build();

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMigrated.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(new Envelope<BuildingWasMigrated>(new Envelope(buildingWasMigrated, metadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Status.Should().Be(buildingWasMigrated.BuildingStatus);
                    buildingVersion.OsloStatus.Should().Be(BuildingStatus.Parse(buildingWasMigrated.BuildingStatus).Map());
                    buildingVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingWasMigrated.ExtendedWkbGeometry.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be(buildingWasMigrated.GeometryMethod);
                    buildingVersion.OsloGeometryMethod.Should().Be(BuildingGeometryMethod.Parse(buildingWasMigrated.GeometryMethod).Map());
                    buildingVersion.IsRemoved.Should().Be(buildingWasMigrated.IsRemoved);
                    buildingVersion.Namespace.Should().Be(BuildingNamespace);
                    buildingVersion.PuriId.Should().Be($"{BuildingNamespace}/{buildingWasMigrated.BuildingPersistentLocalId}");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                    buildingVersion.CreatedOnTimestamp.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                    buildingVersion.BuildingPersistentLocalId.Should().Be(buildingWasMigrated.BuildingPersistentLocalId);
                    buildingVersion.Type.Should().Be("EventName");

                    foreach (var buildingUnit in buildingWasMigrated.BuildingUnits)
                    {
                        var buildingUnitVersion = buildingVersion.BuildingUnits
                            .Single(x => x.BuildingUnitPersistentLocalId == buildingUnit.BuildingUnitPersistentLocalId);

                        buildingUnitVersion.BuildingPersistentLocalId.Should().Be(buildingWasMigrated.BuildingPersistentLocalId);
                        buildingUnitVersion.Status.Should().Be(BuildingUnitStatus.Parse(buildingUnit.Status).Status);
                        buildingUnitVersion.OsloStatus.Should().Be(BuildingUnitStatus.Parse(buildingUnit.Status).Map());
                        buildingUnitVersion.Function.Should().Be(BuildingUnitFunction.Parse(buildingUnit.Function).Function);
                        buildingUnitVersion.OsloFunction.Should().Be(BuildingUnitFunction.Parse(buildingUnit.Function).Map());
                        buildingUnitVersion.GeometryMethod.Should()
                            .Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod).GeometryMethod);
                        buildingUnitVersion.OsloGeometryMethod.Should()
                            .Be(BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod).Map());
                        buildingUnitVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingUnit.ExtendedWkbGeometry.ToByteArray()));
                        buildingUnitVersion.HasDeviation.Should().BeFalse();
                        buildingUnitVersion.IsRemoved.Should().Be(buildingUnit.IsRemoved);
                        buildingUnitVersion.Namespace.Should().Be(BuildingUnitNamespace);
                        buildingUnitVersion.PuriId.Should().Be($"{BuildingUnitNamespace}/{buildingUnitVersion.BuildingUnitPersistentLocalId}");
                        buildingUnitVersion.VersionTimestamp.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                        buildingUnitVersion.CreatedOnTimestamp.Should().Be(buildingWasMigrated.Provenance.Timestamp);
                        buildingUnitVersion.Type.Should().Be("EventName");

                        buildingUnitVersion.Addresses.Should().HaveCount(buildingUnit.AddressPersistentLocalIds.Count);
                        foreach (var addressPersistentLocalId in buildingUnit.AddressPersistentLocalIds)
                        {
                            buildingUnitVersion.Addresses
                                .SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId)
                                .Should().NotBeNull();
                        }
                    }
                });
        }

        [Fact]
        public async Task WhenBuildingWasPlannedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, metadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Status.Should().Be("Planned");
                    buildingVersion.OsloStatus.Should().Be("Gepland");
                    buildingVersion.Geometry.Should().BeEquivalentTo(_wkbReader.Read(buildingWasPlannedV2.ExtendedWkbGeometry.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("Outlined");
                    buildingVersion.OsloGeometryMethod.Should().Be("Ingeschetst");
                    buildingVersion.IsRemoved.Should().BeFalse();
                    buildingVersion.Namespace.Should().Be(BuildingNamespace);
                    buildingVersion.PuriId.Should().Be($"{BuildingNamespace}/{buildingWasPlannedV2.BuildingPersistentLocalId}");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasPlannedV2.Provenance.Timestamp);
                    buildingVersion.CreatedOnTimestamp.Should().Be(buildingWasPlannedV2.Provenance.Timestamp);
                    buildingVersion.BuildingPersistentLocalId.Should().Be(buildingWasPlannedV2.BuildingPersistentLocalId);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenUnplannedBuildingWasRealizedAndMeasured()
        {
            var unplannedBuildingWasRealizedAndMeasured = _fixture.Create<UnplannedBuildingWasRealizedAndMeasured>();

            var position = _fixture.Create<long>();
            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, unplannedBuildingWasRealizedAndMeasured.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(new Envelope<UnplannedBuildingWasRealizedAndMeasured>(new Envelope(unplannedBuildingWasRealizedAndMeasured, metadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Status.Should().Be("Realized");
                    buildingVersion.OsloStatus.Should().Be("Gerealiseerd");
                    buildingVersion.GeometryMethod.Should().Be("MeasuredByGrb");
                    buildingVersion.OsloGeometryMethod.Should().Be("IngemetenGRB");
                    buildingVersion.Geometry.Should()
                        .BeEquivalentTo(_wkbReader.Read(unplannedBuildingWasRealizedAndMeasured.ExtendedWkbGeometry.ToByteArray()));
                    buildingVersion.IsRemoved.Should().BeFalse();
                    buildingVersion.Namespace.Should().Be(BuildingNamespace);
                    buildingVersion.PuriId.Should()
                        .Be($"{BuildingNamespace}/{unplannedBuildingWasRealizedAndMeasured.BuildingPersistentLocalId}");
                    buildingVersion.VersionTimestamp.Should().Be(unplannedBuildingWasRealizedAndMeasured.Provenance.Timestamp);
                    buildingVersion.CreatedOnTimestamp.Should().Be(unplannedBuildingWasRealizedAndMeasured.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(unplannedBuildingWasRealizedAndMeasured.Provenance.Timestamp);
                    buildingVersion.BuildingPersistentLocalId.Should().Be(unplannedBuildingWasRealizedAndMeasured.BuildingPersistentLocalId);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingOutlineWasChanged()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingOutlineWasChanged = new BuildingOutlineWasChanged(
                _fixture.Create<BuildingPersistentLocalId>(),
                new []{ new BuildingUnitPersistentLocalId(buildingUnitWasPlannedV2.BuildingUnitPersistentLocalId) },
                _fixture.Create<ExtendedWkbGeometry>(),
                _fixture.Create<ExtendedWkbGeometry>());
            ((ISetProvenance)buildingOutlineWasChanged).SetProvenance(_fixture.Create<Provenance>());

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingUnitWasPlannedV2Metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingOutlineWasChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingOutlineWasChanged.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(buildingUnitWasPlannedV2, buildingUnitWasPlannedV2Metadata)),
                    new Envelope<BuildingOutlineWasChanged>(new Envelope(buildingOutlineWasChanged, buildingOutlineWasChangedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Geometry.Should()
                        .BeEquivalentTo(_wkbReader.Read(buildingOutlineWasChanged.ExtendedWkbGeometryBuilding.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("Outlined");
                    buildingVersion.OsloGeometryMethod.Should().Be("Ingeschetst");
                    buildingVersion.VersionTimestamp.Should().Be(buildingOutlineWasChanged.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");

                    var buildingUnitVersion = buildingVersion.BuildingUnits.Single();
                    buildingUnitVersion.GeometryMethod.Should().Be("DerivedFromObject");
                    buildingUnitVersion.OsloGeometryMethod.Should().Be("AfgeleidVanObject");
                    buildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingOutlineWasChanged.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    buildingUnitVersion.VersionTimestamp.Should().Be(buildingOutlineWasChanged.Provenance.Timestamp);
                    buildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementWasChanged()
        {
            var position = _fixture.Create<long>();

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var firstBuildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var secondBuildingUnitWasPlannedV2 = _fixture.Create<BuildingUnitWasPlannedV2>();
            var buildingMeasurementWasChanged = new BuildingMeasurementWasChanged(
                _fixture.Create<BuildingPersistentLocalId>(),
                new []{ new BuildingUnitPersistentLocalId(firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId) },
                new []{ new BuildingUnitPersistentLocalId(secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId) },
                _fixture.Create<ExtendedWkbGeometry>(),
                _fixture.Create<ExtendedWkbGeometry>());
            ((ISetProvenance)buildingMeasurementWasChanged).SetProvenance(_fixture.Create<Provenance>());

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var firstBuildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstBuildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var secondBuildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstBuildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingMeasurementWasChangedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingMeasurementWasChanged.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(firstBuildingUnitWasPlannedV2, firstBuildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(secondBuildingUnitWasPlannedV2, secondBuildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingMeasurementWasChanged>(
                        new Envelope(buildingMeasurementWasChanged, buildingMeasurementWasChangedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Geometry.Should()
                        .BeEquivalentTo(_wkbReader.Read(buildingMeasurementWasChanged.ExtendedWkbGeometryBuilding.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("MeasuredByGrb");
                    buildingVersion.OsloGeometryMethod.Should().Be("IngemetenGRB");
                    buildingVersion.VersionTimestamp.Should().Be(buildingMeasurementWasChanged.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingMeasurementWasChanged.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");

                    var firstBuildingUnitVersion = buildingVersion.BuildingUnits.Single(
                        x => x.BuildingUnitPersistentLocalId == firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    firstBuildingUnitVersion.Should().NotBeNull();

                    firstBuildingUnitVersion.GeometryMethod.Should().Be("DerivedFromObject");
                    firstBuildingUnitVersion.OsloGeometryMethod.Should().Be("AfgeleidVanObject");
                    firstBuildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingMeasurementWasChanged.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    firstBuildingUnitVersion.VersionTimestamp.Should().Be(buildingMeasurementWasChanged.Provenance.Timestamp);
                    firstBuildingUnitVersion.Type.Should().Be("EventName");

                    var secondBuildingUnitVersion = buildingVersion.BuildingUnits.Single(
                        x => x.BuildingUnitPersistentLocalId == secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    secondBuildingUnitVersion.Should().NotBeNull();

                    secondBuildingUnitVersion.GeometryMethod.Should().Be("DerivedFromObject");
                    secondBuildingUnitVersion.OsloGeometryMethod.Should().Be("AfgeleidVanObject");
                    secondBuildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingMeasurementWasChanged.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    secondBuildingUnitVersion.VersionTimestamp.Should().Be(buildingMeasurementWasChanged.Provenance.Timestamp);
                    secondBuildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingBecameUnderConstructionV2()
        {
            var position = _fixture.Create<long>();

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingBecameUnderConstructionV2 = _fixture.Create<BuildingBecameUnderConstructionV2>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var buildingBecameUnderConstructionMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingBecameUnderConstructionV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingBecameUnderConstructionV2>(new Envelope(buildingBecameUnderConstructionV2,
                        buildingBecameUnderConstructionMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Status.Should().Be("UnderConstruction");
                    buildingVersion.OsloStatus.Should().Be("InAanbouw");
                    buildingVersion.VersionTimestamp.Should().Be(buildingBecameUnderConstructionV2.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");

                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromUnderConstructionToPlanned()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasCorrectedFromUnderConstructionToPlanned = _fixture.Create<BuildingWasCorrectedFromUnderConstructionToPlanned>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var buildingWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromUnderConstructionToPlanned.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>(
                        new Envelope(buildingWasCorrectedFromUnderConstructionToPlanned, buildingWasCorrectedToPlannedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Status.Should().Be("Planned");
                    buildingVersion.OsloStatus.Should().Be("Gepland");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedFromUnderConstructionToPlanned.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasRealizedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRealizedV2 = _fixture.Create<BuildingWasRealizedV2>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };

            var buildingWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasRealizedV2>(new Envelope(buildingWasRealizedV2, buildingWasRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Status.Should().Be("Realized");
                    buildingVersion.OsloStatus.Should().Be("Gerealiseerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasRealizedV2.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromRealizedToUnderConstruction()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRealizedV2 = _fixture.Create<BuildingWasRealizedV2>();
            var buildingWasCorrectedFromRealizedToUnderConstruction = _fixture.Create<BuildingWasCorrectedFromRealizedToUnderConstruction>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasCorrectedToUnderConstructionMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromRealizedToUnderConstruction.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasRealizedV2>(
                        new Envelope(
                            buildingWasRealizedV2,
                            buildingWasRealizedMetadata)),
                    new Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>(
                        new Envelope(
                            buildingWasCorrectedFromRealizedToUnderConstruction,
                            buildingWasCorrectedToUnderConstructionMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Status.Should().Be("UnderConstruction");
                    buildingVersion.OsloStatus.Should().Be("InAanbouw");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedFromRealizedToUnderConstruction.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasNotRealizedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasNotRealizedV2 = _fixture.Create<BuildingWasNotRealizedV2>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasNotRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };
            await Sut
                .Given(new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasNotRealizedV2>(new Envelope(buildingWasNotRealizedV2, buildingWasNotRealizedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Status.Should().Be("NotRealized");
                    buildingVersion.OsloStatus.Should().Be("NietGerealiseerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasNotRealizedV2.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasCorrectedFromNotRealizedToPlanned()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasNotRealizedV2 = _fixture.Create<BuildingWasNotRealizedV2>();
            var buildingWasCorrectedFromNotRealizedToPlanned = _fixture.Create<BuildingWasCorrectedFromNotRealizedToPlanned>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasNotRealizedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasNotRealizedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasCorrectedToPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasCorrectedFromNotRealizedToPlanned.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasNotRealizedV2>(new Envelope(buildingWasNotRealizedV2, buildingWasNotRealizedMetadata)),
                    new Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>(
                        new Envelope(buildingWasCorrectedFromNotRealizedToPlanned, buildingWasCorrectedToPlannedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Status.Should().Be("Planned");
                    buildingVersion.OsloStatus.Should().Be("Gepland");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasCorrectedFromNotRealizedToPlanned.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasRemovedV2()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasRemovedV2 = _fixture.Create<BuildingWasRemovedV2>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasRemovedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasRemovedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasRemovedV2>(
                        new Envelope(
                            buildingWasRemovedV2,
                            buildingWasRemovedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.IsRemoved.Should().BeTrue();
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasRemovedV2.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasMeasured()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
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

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var firstBuildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstBuildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var secondBuildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstBuildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasMeasuredMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasMeasured.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(firstBuildingUnitWasPlannedV2, firstBuildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(secondBuildingUnitWasPlannedV2, secondBuildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingWasMeasured>(new Envelope(buildingWasMeasured, buildingWasMeasuredMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Geometry.Should()
                        .BeEquivalentTo(_wkbReader.Read(buildingWasMeasured.ExtendedWkbGeometryBuilding.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("MeasuredByGrb");
                    buildingVersion.OsloGeometryMethod.Should().Be("IngemetenGRB");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasMeasured.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingWasMeasured.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");

                    var firstBuildingUnitVersion = buildingVersion.BuildingUnits.Single(
                        x => x.BuildingUnitPersistentLocalId == firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    firstBuildingUnitVersion.Should().NotBeNull();

                    firstBuildingUnitVersion.GeometryMethod.Should().Be("DerivedFromObject");
                    firstBuildingUnitVersion.OsloGeometryMethod.Should().Be("AfgeleidVanObject");
                    firstBuildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingWasMeasured.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    firstBuildingUnitVersion.VersionTimestamp.Should().Be(buildingWasMeasured.Provenance.Timestamp);
                    firstBuildingUnitVersion.Type.Should().Be("EventName");

                    var secondBuildingUnitVersion = buildingVersion.BuildingUnits.Single(
                    x => x.BuildingUnitPersistentLocalId == secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    secondBuildingUnitVersion.Should().NotBeNull();

                    secondBuildingUnitVersion.GeometryMethod.Should().Be("DerivedFromObject");
                    secondBuildingUnitVersion.OsloGeometryMethod.Should().Be("AfgeleidVanObject");
                    secondBuildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingWasMeasured.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    secondBuildingUnitVersion.VersionTimestamp.Should().Be(buildingWasMeasured.Provenance.Timestamp);
                    secondBuildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingMeasurementWasCorrected()
        {
            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
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

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var firstBuildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstBuildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var secondBuildingUnitWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, firstBuildingUnitWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingMeasurementWasCorrectedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingMeasurementWasCorrected.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(new Envelope(buildingWasPlannedV2, buildingWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(firstBuildingUnitWasPlannedV2, firstBuildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingUnitWasPlannedV2>(new Envelope(secondBuildingUnitWasPlannedV2, secondBuildingUnitWasPlannedMetadata)),
                    new Envelope<BuildingMeasurementWasCorrected>(new Envelope(buildingMeasurementWasCorrected, buildingMeasurementWasCorrectedMetadata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Geometry.Should()
                        .BeEquivalentTo(_wkbReader.Read(buildingMeasurementWasCorrected.ExtendedWkbGeometryBuilding.ToByteArray()));
                    buildingVersion.GeometryMethod.Should().Be("MeasuredByGrb");
                    buildingVersion.OsloGeometryMethod.Should().Be("IngemetenGRB");
                    buildingVersion.VersionTimestamp.Should().Be(buildingMeasurementWasCorrected.Provenance.Timestamp);
                    buildingVersion.LastChangedOnTimestamp.Should().Be(buildingMeasurementWasCorrected.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");

                    var firstBuildingUnitVersion = buildingVersion.BuildingUnits.Single(
                        x => x.BuildingUnitPersistentLocalId == firstBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    firstBuildingUnitVersion.Should().NotBeNull();

                    firstBuildingUnitVersion.GeometryMethod.Should().Be("DerivedFromObject");
                    firstBuildingUnitVersion.OsloGeometryMethod.Should().Be("AfgeleidVanObject");
                    firstBuildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingMeasurementWasCorrected.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    firstBuildingUnitVersion.VersionTimestamp.Should().Be(buildingMeasurementWasCorrected.Provenance.Timestamp);
                    firstBuildingUnitVersion.Type.Should().Be("EventName");

                    var secondBuildingUnitVersion = buildingVersion.BuildingUnits.Single(
                        x => x.BuildingUnitPersistentLocalId == secondBuildingUnitWasPlannedV2.BuildingUnitPersistentLocalId);
                    secondBuildingUnitVersion.Should().NotBeNull();

                    secondBuildingUnitVersion.GeometryMethod.Should().Be("DerivedFromObject");
                    secondBuildingUnitVersion.OsloGeometryMethod.Should().Be("AfgeleidVanObject");
                    secondBuildingUnitVersion.Geometry.Should().BeEquivalentTo(
                        _wkbReader.Read(buildingMeasurementWasCorrected.ExtendedWkbGeometryBuildingUnits!.ToByteArray()));
                    secondBuildingUnitVersion.VersionTimestamp.Should().Be(buildingMeasurementWasCorrected.Provenance.Timestamp);
                    secondBuildingUnitVersion.Type.Should().Be("EventName");
                });
        }

        [Fact]
        public async Task WhenBuildingWasDemolished()
        {
            _fixture.Customize(new WithFixedBuildingPersistentLocalId());
            _fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());

            var buildingWasPlannedV2 = _fixture.Create<BuildingWasPlannedV2>();
            var buildingWasDemolished = _fixture.Create<BuildingWasDemolished>();

            var position = _fixture.Create<long>();

            var buildingWasPlannedMetadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasPlannedV2.GetHash() },
                { Envelope.PositionMetadataKey, position },
                { Envelope.EventNameMetadataKey, _fixture.Create<string>()}
            };
            var buildingWasDemolishedMetdata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, buildingWasDemolished.GetHash() },
                { Envelope.PositionMetadataKey, ++position },
                { Envelope.EventNameMetadataKey, "EventName"}
            };

            await Sut
                .Given(
                    new Envelope<BuildingWasPlannedV2>(
                        new Envelope(
                            buildingWasPlannedV2,
                            buildingWasPlannedMetadata)),
                    new Envelope<BuildingWasDemolished>(
                        new Envelope(
                            buildingWasDemolished,
                            buildingWasDemolishedMetdata)))
                .Then(async ct =>
                {
                    var buildingVersion = await ct.BuildingVersions.FindAsync(position);
                    buildingVersion.Should().NotBeNull();

                    buildingVersion!.Status.Should().Be("Retired");
                    buildingVersion.OsloStatus.Should().Be("Gehistoreerd");
                    buildingVersion.VersionTimestamp.Should().Be(buildingWasDemolished.Provenance.Timestamp);
                    buildingVersion.Type.Should().Be("EventName");
                });
        }

        // [Fact]
        // public async Task WhenBuildingMergerWasRealized()
        // {
        //     var buildingMergerWasRealized = _fixture.Create<BuildingMergerWasRealized>();
        //
        //     var position = _fixture.Create<long>();
        //
        //     var buildingMergerWasRealizedMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, buildingMergerWasRealized.GetHash() },
        //         { Envelope.PositionMetadataKey, position }
        //     };
        //
        //     await Sut
        //         .Given(
        //             new Envelope<BuildingMergerWasRealized>(
        //                 new Envelope(
        //                     buildingMergerWasRealized,
        //                     buildingMergerWasRealizedMetadata)))
        //         .Then(async ct =>
        //         {
        //             var buildingVersion = await ct.BuildingVersions.FindAsync(position);
        //             buildingVersion.Should().NotBeNull();
        //
        //             buildingVersion!.BuildingPersistentLocalId.Should().Be(buildingMergerWasRealized.BuildingPersistentLocalId);
        //             buildingVersion.Status.Should().Be("Realized");
        //             buildingVersion.OsloStatus.Should().Be("Gerealiseerd");
        //             buildingVersion.Geometry.Should()
        //                 .BeEquivalentTo(_wkbReader.Read(buildingMergerWasRealized.ExtendedWkbGeometry.ToByteArray()));
        //             buildingVersion.GeometryMethod.Should().Be("MeasuredByGrb");
        //             buildingVersion.OsloGeometryMethod.Should().Be("IngemetenGRB");
        //             buildingVersion.VersionTimestamp.Should().Be(buildingMergerWasRealized.Provenance.Timestamp);
        //             buildingVersion.CreatedOnTimestamp.Should().Be(buildingMergerWasRealized.Provenance.Timestamp);
        //             buildingVersion.LastChangedOnTimestamp.Should().Be(buildingMergerWasRealized.Provenance.Timestamp);
        //         });
        // }
        //
        // [Fact]
        // public async Task WhenBuildingWasMerged()
        // {
        //     _fixture.Customize(new WithFixedBuildingPersistentLocalId());
        //
        //     var buildingWasPlanned = _fixture.Create<BuildingWasPlannedV2>();
        //     var buildingWasMerged = _fixture.Create<BuildingWasMerged>();
        //
        //     var position = _fixture.Create<long>();
        //
        //     var buildingWasPlannedMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, buildingWasPlanned.GetHash() },
        //         { Envelope.PositionMetadataKey, position }
        //     };
        //     var buildingWasMergedMetadata = new Dictionary<string, object>
        //     {
        //         { AddEventHashPipe.HashMetadataKey, buildingWasMerged.GetHash() },
        //         { Envelope.PositionMetadataKey, ++position }
        //     };
        //
        //     await Sut
        //         .Given(
        //             new Envelope<BuildingWasPlannedV2>(
        //                 new Envelope(
        //                     buildingWasPlanned,
        //                     buildingWasPlannedMetadata)),
        //             new Envelope<BuildingWasMerged>(
        //                 new Envelope(
        //                     buildingWasMerged,
        //                     buildingWasMergedMetadata)))
        //         .Then(async ct =>
        //         {
        //             var buildingVersion = await ct.BuildingVersions.FindAsync(position);
        //             buildingVersion.Should().NotBeNull();
        //
        //             buildingVersion!.Status.Should().Be("Retired");
        //             buildingVersion.OsloStatus.Should().Be("Gehistoreerd");
        //             buildingVersion.VersionTimestamp.Should().Be(buildingWasMerged.Provenance.Timestamp);
        //         });
        // }

        protected override BuildingVersionProjections CreateProjection() =>
            new BuildingVersionProjections(
                new OptionsWrapper<IntegrationOptions>(new IntegrationOptions
                {
                    BuildingNamespace = BuildingNamespace,
                    BuildingUnitNamespace = BuildingUnitNamespace
                }),
                _persistentLocalIdFinder.Object,
                _addresses.Object);
    }
}
