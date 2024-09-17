namespace BuildingRegistry.Tests.ProjectionTests.Consumer.Parcel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.ParcelRegistry;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using BuildingRegistry.Consumer.Read.Parcel;
    using BuildingRegistry.Consumer.Read.Parcel.ParcelWithCount;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Projections.Legacy;
    using Tests.Legacy.Autofixture;
    using Xunit;
    using Xunit.Abstractions;

    public class ParcelConsumerKafkaProjectionTests : KafkaProjectionTest<ConsumerParcelContext, ParcelKafkaProjection>
    {
        private readonly ParcelWasMigrated _parcelWasMigrated;

        private readonly Mock<IBuildingMatching> _buildingMatchingMock = new();

        private readonly int _addressPersistentLocalId;
        private static ParcelAddressWasDetachedV2 _parcelAddressWasDetachedV2 = null!;
        private static ParcelAddressWasDetachedBecauseAddressWasRejected _parcelAddressWasDetachedBecauseAddressWasRejected = null!;
        private static ParcelAddressWasDetachedBecauseAddressWasRemoved _parcelAddressWasDetachedBecauseAddressWasRemoved = null!;
        private static ParcelAddressWasDetachedBecauseAddressWasRetired _parcelAddressWasDetachedBecauseAddressWasRetired = null!;
        public static IEnumerable<object[]> DetachedEvents
        {
            get
            {
                yield return [new Func<object>(() => _parcelAddressWasDetachedV2)];
                yield return [new Func<object>(() => _parcelAddressWasDetachedBecauseAddressWasRejected)];
                yield return [new Func<object>(() => _parcelAddressWasDetachedBecauseAddressWasRemoved)];
                yield return [new Func<object>(() => _parcelAddressWasDetachedBecauseAddressWasRetired)];
            }
        }

        public ParcelConsumerKafkaProjectionTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customizations.Add(new WithUniqueInteger());

            var parcelStatus = Fixture
                .Build<ParcelStatus>()
                .FromFactory(() =>
                {
                    var statuses = new List<ParcelStatus>
                    {
                        ParcelStatus.Realized, ParcelStatus.Retired
                    };

                    return statuses[new Random(Fixture.Create<int>()).Next(0, statuses.Count - 1)];
                })
                .Create();

            _parcelWasMigrated = new ParcelWasMigrated(
                Fixture.Create<Guid>().ToString("D"),
                Fixture.Create<Guid>().ToString("D"),
                Fixture.Create<string>(),
                parcelStatus.Status,
                Fixture.Create<bool>(),
                Fixture.Create<IEnumerable<int>>(),
                GeometryHelper.ValidPolygon.AsBinary().ToHexString(),
                Fixture.Create<Provenance>());

            _addressPersistentLocalId = Fixture.Create<int>();

            _parcelAddressWasDetachedV2 = new ParcelAddressWasDetachedV2(
                _parcelWasMigrated.ParcelId,
                _parcelWasMigrated.CaPaKey,
                _addressPersistentLocalId,
                Fixture.Create<Provenance>());
            _parcelAddressWasDetachedBecauseAddressWasRejected = new ParcelAddressWasDetachedBecauseAddressWasRejected(
                _parcelWasMigrated.ParcelId,
                _parcelWasMigrated.CaPaKey,
                _addressPersistentLocalId,
                Fixture.Create<Provenance>());
            _parcelAddressWasDetachedBecauseAddressWasRemoved = new ParcelAddressWasDetachedBecauseAddressWasRemoved(
                _parcelWasMigrated.ParcelId,
                _parcelWasMigrated.CaPaKey,
                _addressPersistentLocalId,
                Fixture.Create<Provenance>());
            _parcelAddressWasDetachedBecauseAddressWasRetired = new ParcelAddressWasDetachedBecauseAddressWasRetired(
                _parcelWasMigrated.ParcelId,
                _parcelWasMigrated.CaPaKey,
                _addressPersistentLocalId,
                Fixture.Create<Provenance>());
        }

        [Fact]
        public async Task ParcelWasMigrated_AddsParcel()
        {
            var underlyingBuildingId = (int)Fixture.Create<BuildingPersistentLocalId>();

            _buildingMatchingMock
                .Setup(x => x.GetUnderlyingBuildings(It.IsAny<Geometry>()))
                .Returns(new[] { underlyingBuildingId });

            Given(_parcelWasMigrated);

            await Then(async context =>
            {
                var parcelId = Guid.Parse(_parcelWasMigrated.ParcelId);
                var parcel =
                    await context.ParcelConsumerItemsWithCount.FindAsync(parcelId);

                parcel.Should().NotBeNull();
                parcel!.CaPaKey.Should().Be(_parcelWasMigrated.CaPaKey);
                parcel.Status.Should().Be(ParcelStatus.Parse(_parcelWasMigrated.ParcelStatus));
                parcel.IsRemoved.Should().Be(parcel.IsRemoved);

                foreach (var addressPersistentLocalId in _parcelWasMigrated.AddressPersistentLocalIds)
                {
                    var parcelAddressItem =
                        await context.ParcelAddressItemsWithCount.FindAsync(parcelId, addressPersistentLocalId);

                    parcelAddressItem.Should().NotBeNull();
                }

                var buildingsToInvalidate = context.BuildingsToInvalidate.Local.ToList();
                buildingsToInvalidate.Should().HaveCount(1);
                buildingsToInvalidate.Single().BuildingPersistentLocalId.Should().Be(underlyingBuildingId);
            });
        }

        [Fact]
        public async Task ParcelAddressWasAttachedV2_AddsParcelAddress()
        {
            var parcelAddressWasAttachedV2 = Fixture
                .Build<ParcelAddressWasAttachedV2>()
                .FromFactory(() => new ParcelAddressWasAttachedV2(
                    _parcelWasMigrated.ParcelId,
                    _parcelWasMigrated.CaPaKey,
                    Fixture.Create<int>(),
                    Fixture.Create<Provenance>()))
                .Create();

            Given(_parcelWasMigrated, parcelAddressWasAttachedV2);

            await Then(async context =>
            {
                var parcelId = Guid.Parse(_parcelWasMigrated.ParcelId);
                var parcel =
                    await context.ParcelConsumerItemsWithCount.FindAsync(parcelId);

                parcel.Should().NotBeNull();

                var parcelAddressItem =
                    await context.ParcelAddressItemsWithCount.FindAsync(parcelId, parcelAddressWasAttachedV2.AddressPersistentLocalId);

                parcelAddressItem.Should().NotBeNull();
            });
        }

        [Theory]
        [MemberData(nameof(DetachedEvents))]
        public async Task ParcelAddressWasDetachedV2_RemovesParcelAddress(Func<object> addressDetachedFactory)
        {
            var parcelAddressWasAttachedV2 = Fixture
                .Build<ParcelAddressWasAttachedV2>()
                .FromFactory(() => new ParcelAddressWasAttachedV2(
                    _parcelWasMigrated.ParcelId,
                    _parcelWasMigrated.CaPaKey,
                    _addressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();

            Given(_parcelWasMigrated, parcelAddressWasAttachedV2, addressDetachedFactory());

            await Then(async context =>
            {
                var parcelId = Guid.Parse(_parcelWasMigrated.ParcelId);
                var parcel = await context.ParcelConsumerItemsWithCount.FindAsync(parcelId);

                parcel.Should().NotBeNull();

                var parcelAddressItem = await context.ParcelAddressItemsWithCount.FindAsync(parcelId, _addressPersistentLocalId);
                parcelAddressItem.Should().BeNull();
            });
        }

        [Fact]
        public async Task ParcelAddressWasReplacedBecauseAddressWasReaddressed_ReplacesParcelAddress()
        {
            var parcelAddressWasAttachedV2 = Fixture
                .Build<ParcelAddressWasAttachedV2>()
                .FromFactory(() => new ParcelAddressWasAttachedV2(
                    _parcelWasMigrated.ParcelId,
                    _parcelWasMigrated.CaPaKey,
                    Fixture.Create<int>(),
                    Fixture.Create<Provenance>()))
                .Create();

            var secondParcelAddressWasAttachedV2 = Fixture
                .Build<ParcelAddressWasAttachedV2>()
                .FromFactory(() => new ParcelAddressWasAttachedV2(
                    _parcelWasMigrated.ParcelId,
                    _parcelWasMigrated.CaPaKey,
                    Fixture.Create<int>(),
                    Fixture.Create<Provenance>()))
                .Create();

            var parcelAddressWasReplacedBecauseAddressWasReaddressed = Fixture
                .Build<ParcelAddressWasReplacedBecauseAddressWasReaddressed>()
                .FromFactory(() => new ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                    _parcelWasMigrated.ParcelId,
                    _parcelWasMigrated.CaPaKey,
                    secondParcelAddressWasAttachedV2.AddressPersistentLocalId,
                    parcelAddressWasAttachedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();

            Given(_parcelWasMigrated,
                parcelAddressWasAttachedV2,
                secondParcelAddressWasAttachedV2,
                parcelAddressWasReplacedBecauseAddressWasReaddressed);

            await Then(async context =>
            {
                var parcelId = Guid.Parse(_parcelWasMigrated.ParcelId);
                var parcel =
                    await context.ParcelConsumerItemsWithCount.FindAsync(parcelId);

                parcel.Should().NotBeNull();

                var previousParcelAddressItem =
                    await context.ParcelAddressItemsWithCount.FindAsync(parcelId,
                        parcelAddressWasReplacedBecauseAddressWasReaddressed.PreviousAddressPersistentLocalId);

                previousParcelAddressItem.Should().BeNull();

                var newParcelAddressItem =
                    await context.ParcelAddressItemsWithCount.FindAsync(parcelId,
                        parcelAddressWasReplacedBecauseAddressWasReaddressed.NewAddressPersistentLocalId);

                newParcelAddressItem.Should().NotBeNull();
                newParcelAddressItem!.Count.Should().Be(2);
            });
        }

        [Fact]
        public async Task ParcelAddressesWereReaddressed_ReplacesParcelAddresses()
        {
            var parcelAddressWasAttachedV2 = Fixture
                .Build<ParcelAddressWasAttachedV2>()
                .FromFactory(() => new ParcelAddressWasAttachedV2(
                    _parcelWasMigrated.ParcelId,
                    _parcelWasMigrated.CaPaKey,
                    Fixture.Create<int>(),
                    Fixture.Create<Provenance>()))
                .Create();

            var parcelAddressesWereReaddressed = Fixture
                .Build<ParcelAddressesWereReaddressed>()
                .FromFactory(() => new ParcelAddressesWereReaddressed(
                    _parcelWasMigrated.ParcelId,
                    _parcelWasMigrated.CaPaKey,
                    [parcelAddressWasAttachedV2.AddressPersistentLocalId],
                    [parcelAddressWasAttachedV2.AddressPersistentLocalId + 1],
                    Fixture.CreateMany<AddressRegistryReaddress>(),
                    Fixture.Create<Provenance>()))
                .Create();

            Given(_parcelWasMigrated, parcelAddressesWereReaddressed);

            await Then(async context =>
            {
                var parcelId = Guid.Parse(_parcelWasMigrated.ParcelId);
                var parcel =
                    await context.ParcelConsumerItemsWithCount.FindAsync(parcelId);

                parcel.Should().NotBeNull();

                foreach (var addressPersistentLocalId in parcelAddressesWereReaddressed.AttachedAddressPersistentLocalIds)
                {
                    var parcelAddressItem = await context.ParcelAddressItemsWithCount.FindAsync(parcelId, addressPersistentLocalId);
                    parcelAddressItem.Should().NotBeNull();
                }

                foreach (var addressPersistentLocalId in parcelAddressesWereReaddressed.DetachedAddressPersistentLocalIds)
                {
                    var parcelAddressItem = await context.ParcelAddressItemsWithCount.FindAsync(parcelId, addressPersistentLocalId);
                    parcelAddressItem.Should().BeNull();
                }
            });
        }

        [Fact]
        public async Task ParcelWasRetiredV2_SetsParcelStatusToRetired()
        {
            var parcelId = Fixture.Create<Guid>().ToString("D");
            var capakey = Fixture.Create<Guid>().ToString("D");
            var underlyingBuildingId = (int)Fixture.Create<BuildingPersistentLocalId>();

            _buildingMatchingMock
                .SetupSequence(x => x.GetUnderlyingBuildings(It.IsAny<Geometry>()))
                .Returns(Array.Empty<int>())
                .Returns(new[] { underlyingBuildingId });

            var parcelWasImported = Fixture
                .Build<ParcelWasImported>()
                .FromFactory(() => new ParcelWasImported(
                    parcelId,
                    capakey,
                    GeometryHelper.ValidPolygon.AsBinary().ToHexString(),
                    Fixture.Create<Provenance>()
                ))
                .Create();

            var parcelWasRetiredV2 = Fixture
                .Build<ParcelWasRetiredV2>()
                .FromFactory(() => new ParcelWasRetiredV2(
                    parcelId,
                    capakey,
                    Fixture.Create<Provenance>()
                ))
                .Create();

            Given(parcelWasImported, parcelWasRetiredV2);

            await Then(async context =>
            {
                var parcel =
                    await context.ParcelConsumerItemsWithCount.FindAsync(Guid.Parse(parcelWasRetiredV2.ParcelId));

                parcel.Should().NotBeNull();
                parcel!.CaPaKey.Should().Be(parcelWasRetiredV2.CaPaKey);
                parcel.Status.Should().Be(ParcelStatus.Retired);
                parcel.IsRemoved.Should().Be(parcel.IsRemoved);

                var buildingsToInvalidate = context.BuildingsToInvalidate.Local.ToList();
                buildingsToInvalidate.Should().HaveCount(1);
                buildingsToInvalidate.Single().BuildingPersistentLocalId.Should().Be(underlyingBuildingId);
            });
        }

        [Fact]
        public async Task ParcelWasCorrectedFromRetiredToRealized_SetsParcelStatusToRealized()
        {
            var parcelId = Fixture.Create<Guid>().ToString("D");
            var capakey = Fixture.Create<Guid>().ToString("D");
            var parcelWasImported = Fixture
                .Build<ParcelWasImported>()
                .FromFactory(() => new ParcelWasImported(
                    parcelId,
                    capakey,
                    GeometryHelper.ValidPolygon.AsBinary().ToHexString(),
                    Fixture.Create<Provenance>()
                ))
                .Create();

            var parcelWasRetiredV2 = Fixture
                .Build<ParcelWasRetiredV2>()
                .FromFactory(() => new ParcelWasRetiredV2(
                    parcelId,
                    capakey,
                    Fixture.Create<Provenance>()
                ))
                .Create();

            var parcelWasCorrectedFromRetiredToRealized = Fixture
                .Build<ParcelWasCorrectedFromRetiredToRealized>()
                .FromFactory(() => new ParcelWasCorrectedFromRetiredToRealized(
                    parcelId,
                    capakey,
                    GeometryHelper.ValidPolygon.AsBinary().ToHexString(),
                    Fixture.Create<Provenance>()
                ))
                .Create();

            var underlyingBuildingId = (int)Fixture.Create<BuildingPersistentLocalId>();

            _buildingMatchingMock
                .SetupSequence(x => x.GetUnderlyingBuildings(It.IsAny<Geometry>()))
                .Returns(Array.Empty<int>())
                .Returns(Array.Empty<int>())
                .Returns(new[] { underlyingBuildingId });

            Given(parcelWasImported, parcelWasRetiredV2, parcelWasCorrectedFromRetiredToRealized);

            await Then(async context =>
            {
                var parcel =
                    await context.ParcelConsumerItemsWithCount.FindAsync(Guid.Parse(parcelWasCorrectedFromRetiredToRealized.ParcelId));

                parcel.Should().NotBeNull();
                parcel!.CaPaKey.Should().Be(parcelWasCorrectedFromRetiredToRealized.CaPaKey);
                parcel.Status.Should().Be(ParcelStatus.Realized);
                parcel.IsRemoved.Should().Be(parcel.IsRemoved);

                var buildingsToInvalidate = context.BuildingsToInvalidate.Local.ToList();
                buildingsToInvalidate.Should().HaveCount(1);
                buildingsToInvalidate.Single().BuildingPersistentLocalId.Should().Be(underlyingBuildingId);
            });
        }

        [Fact]
        public async Task ParcelGeometryWasChanged_SetsParcelStatusToRealized()
        {
             var parcelId = Fixture.Create<Guid>().ToString("D");
             var capakey = Fixture.Create<Guid>().ToString("D");
             var parcelWasImported = Fixture
                 .Build<ParcelWasImported>()
                 .FromFactory(() => new ParcelWasImported(
                     parcelId,
                     capakey,
                     GeometryHelper.ValidPolygon.AsBinary().ToHexString(),
                     Fixture.Create<Provenance>()
                 ))
                 .Create();

             var newGeometry = new WKTReader().Read("POLYGON ((30 10, 10 20, 20 40, 40 40, 30 10))");
             var parcelGeometryWasChanged = Fixture
                 .Build<ParcelGeometryWasChanged>()
                 .FromFactory(() => new ParcelGeometryWasChanged(
                     parcelId,
                     capakey,
                     newGeometry.AsBinary().ToHexString(),
                     Fixture.Create<Provenance>()
                 ))
                 .Create();

             var previousUnderlyingBuildingId = 1;
             var newUnderlyingBuildingId = 2;
             var commonUnderlyingBuildingId = 3;

             _buildingMatchingMock
                 .SetupSequence(x => x.GetUnderlyingBuildings(It.IsAny<Geometry>()))
                 .Returns(Array.Empty<int>())
                 .Returns(new[] { previousUnderlyingBuildingId, commonUnderlyingBuildingId })
                 .Returns(new[] { newUnderlyingBuildingId, commonUnderlyingBuildingId });

             Given(parcelWasImported, parcelGeometryWasChanged);

             await Then(async context =>
             {
                 var parcel =
                     await context.ParcelConsumerItemsWithCount.FindAsync(Guid.Parse(parcelGeometryWasChanged.ParcelId));

                 parcel.Should().NotBeNull();
                 parcel!.Geometry.Should().BeEquivalentTo(newGeometry);

                 var buildingsToInvalidate = context.BuildingsToInvalidate.Local.ToList();
                 buildingsToInvalidate.Should().HaveCount(2);
                 buildingsToInvalidate[0].BuildingPersistentLocalId.Should().Be(previousUnderlyingBuildingId);
                 buildingsToInvalidate[1].BuildingPersistentLocalId.Should().Be(newUnderlyingBuildingId);
             });
        }

        [Fact]
        public async Task ParcelWasImported_AddsParcel()
        {
            var parcelWasImported = Fixture
                .Build<ParcelWasImported>()
                .FromFactory(() => new ParcelWasImported(
                    Fixture.Create<Guid>().ToString("D"),
                    Fixture.Create<Guid>().ToString("D"),
                    GeometryHelper.ValidPolygon.AsBinary().ToHexString(),
                    Fixture.Create<Provenance>()
                ))
                .Create();

            var underlyingBuildingId = (int)Fixture.Create<BuildingPersistentLocalId>();

            _buildingMatchingMock
                .Setup(x => x.GetUnderlyingBuildings(It.IsAny<Geometry>()))
                .Returns(new[] { underlyingBuildingId });

            Given(parcelWasImported);

            await Then(async context =>
            {
                var parcel =
                    await context.ParcelConsumerItemsWithCount.FindAsync(Guid.Parse(parcelWasImported.ParcelId));

                parcel.Should().NotBeNull();
                parcel!.CaPaKey.Should().Be(parcelWasImported.CaPaKey);
                parcel.Status.Should().Be(ParcelStatus.Realized);
                parcel.IsRemoved.Should().Be(parcel.IsRemoved);

                var buildingsToInvalidate = context.BuildingsToInvalidate.Local.ToList();
                buildingsToInvalidate.Should().HaveCount(1);
                buildingsToInvalidate.Single().BuildingPersistentLocalId.Should().Be(underlyingBuildingId);
            });
        }

        [Fact]
        public async Task ParcelAddressWasReplacedBecauseOfMunicipalityMerger_ReplacesParcelAddress()
        {
            var parcelAddressWasAttachedV2 = Fixture
                .Build<ParcelAddressWasAttachedV2>()
                .FromFactory(() => new ParcelAddressWasAttachedV2(
                    _parcelWasMigrated.ParcelId,
                    _parcelWasMigrated.CaPaKey,
                    Fixture.Create<int>(),
                    Fixture.Create<Provenance>()))
                .Create();

            var parcelAddressWasReplacedBecauseAddressWasReaddressed = Fixture
                .Build<ParcelAddressWasReplacedBecauseOfMunicipalityMerger>()
                .FromFactory(() => new ParcelAddressWasReplacedBecauseOfMunicipalityMerger(
                    _parcelWasMigrated.ParcelId,
                    _parcelWasMigrated.CaPaKey,
                    Fixture.Create<AddressPersistentLocalId>(),
                    parcelAddressWasAttachedV2.AddressPersistentLocalId,
                    Fixture.Create<Provenance>()))
                .Create();

            Given(_parcelWasMigrated,
                parcelAddressWasAttachedV2,
                parcelAddressWasReplacedBecauseAddressWasReaddressed);

            await Then(async context =>
            {
                var parcelId = Guid.Parse(_parcelWasMigrated.ParcelId);
                var parcel = await context.ParcelConsumerItemsWithCount.FindAsync(parcelId);

                parcel.Should().NotBeNull();

                var previousParcelAddressItem =
                    await context.ParcelAddressItemsWithCount.FindAsync(parcelId,
                        parcelAddressWasReplacedBecauseAddressWasReaddressed.PreviousAddressPersistentLocalId);

                previousParcelAddressItem.Should().BeNull();

                var newParcelAddressItem =
                    await context.ParcelAddressItemsWithCount.FindAsync(parcelId,
                        parcelAddressWasReplacedBecauseAddressWasReaddressed.NewAddressPersistentLocalId);

                newParcelAddressItem.Should().NotBeNull();
                newParcelAddressItem!.Count.Should().Be(1);
            });
        }

        protected override ConsumerParcelContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ConsumerParcelContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ConsumerParcelContext(options);
        }

        protected override ParcelKafkaProjection CreateProjection() => new ParcelKafkaProjection(Container);

        protected override void ConfigureCommandHandling(ContainerBuilder builder)
        {
            base.ConfigureCommandHandling(builder);

            builder.Register(c => _buildingMatchingMock.Object);
        }
    }
}
