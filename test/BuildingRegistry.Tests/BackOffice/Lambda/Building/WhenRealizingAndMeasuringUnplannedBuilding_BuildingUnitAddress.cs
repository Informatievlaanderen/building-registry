namespace BuildingRegistry.Tests.BackOffice.Lambda.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using BuildingRegistry.Building.Datastructures;
    using BuildingRegistry.Building.Events;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NetTopologySuite.Geometries;
    using NodaTime;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using TicketingService.Abstractions;
    using Xunit;

    public partial class WhenRealizingAndMeasuringUnplannedBuilding
    {
        [Fact]
        public async Task MainBuildingWithRemovedAddress_ThenNotRealizeUnplannedBuildingUnit()
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var buildings = new Mock<IBuildings>();
            buildings
                .Setup(x => x.GetAsync(It.IsAny<BuildingStreamId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Building(NoSnapshotStrategy.Instance));

            var singleUnderlyingParcelAddress = new AddressData(new AddressPersistentLocalId(123), AddressStatus.Current, true);

            var parcelMatching = new Mock<IParcelMatching>();
            parcelMatching
                .Setup(x => x.GetUnderlyingParcels(It.IsAny<Geometry>()))
                .ReturnsAsync(new List<ParcelData>
                {
                    new ParcelData(
                        Guid.NewGuid(),
                        string.Empty,
                        GeometryHelper.ValidPolygon,
                        string.Empty,
                        new List<AddressPersistentLocalId>
                        {
                            singleUnderlyingParcelAddress.AddressPersistentLocalId
                        })
                });

            var addresses = new Mock<IAddresses>();
            addresses.Setup(x => x.GetAddresses(new List<AddressPersistentLocalId>
                {
                    singleUnderlyingParcelAddress.AddressPersistentLocalId
                }))
                .ReturnsAsync(new List<AddressData> { singleUnderlyingParcelAddress });

            var handler = new RealizeAndMeasureUnplannedBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler.Object,
                buildings.Object,
                parcelMatching.Object,
                addresses.Object,
                _backOfficeContext,
                _persistentLocalIdGenerator.Object,
                Container);

            //Act
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            await handler.Handle(
                new RealizeAndMeasureUnplannedBuildingLambdaRequest(
                    buildingPersistentLocalId,
                    new RealizeAndMeasureUnplannedBuildingSqsRequest
                    {
                        BuildingPersistentLocalId = buildingPersistentLocalId,
                        IfMatchHeaderValue = null,
                        Metadata = new Dictionary<string, object?>(),
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        TicketId = Guid.NewGuid(),
                        Request = new RealizeAndMeasureUnplannedBuildingRequest
                        {
                            GrbData = new GrbData
                            {
                                GrbObjectType = "1",
                                VersionDate = SystemClock.Instance.GetCurrentInstant().ToString(),
                                GeometriePolygoon =
                                    "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                            },
                        }
                    }),
                CancellationToken.None);

            // Assert
            idempotentCommandHandler.Verify(x => x.Dispatch(
                It.IsAny<Guid>(),
                It.IsAny<RealizeUnplannedBuildingUnit>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public async Task MainBuildingWithNotActiveAddresses_ThenNotRealizeUnplannedBuildingUnit(AddressStatus status)
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var buildings = new Mock<IBuildings>();
            buildings
                .Setup(x => x.GetAsync(It.IsAny<BuildingStreamId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Building(NoSnapshotStrategy.Instance));

            var singleUnderlyingParcelAddress = new AddressData(new AddressPersistentLocalId(123), status, false);

            var parcelMatching = new Mock<IParcelMatching>();
            parcelMatching
                .Setup(x => x.GetUnderlyingParcels(It.IsAny<Geometry>()))
                .ReturnsAsync(new List<ParcelData>
                {
                    new ParcelData(
                        Guid.NewGuid(),
                        string.Empty,
                        GeometryHelper.ValidPolygon,
                        string.Empty,
                        new List<AddressPersistentLocalId>
                        {
                            singleUnderlyingParcelAddress.AddressPersistentLocalId
                        })
                });

            var addresses = new Mock<IAddresses>();
            addresses.Setup(x => x.GetAddresses(new List<AddressPersistentLocalId>
                {
                    singleUnderlyingParcelAddress.AddressPersistentLocalId
                }))
                .ReturnsAsync(new List<AddressData> { singleUnderlyingParcelAddress });

            var handler = new RealizeAndMeasureUnplannedBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler.Object,
                buildings.Object,
                parcelMatching.Object,
                addresses.Object,
                _backOfficeContext,
                _persistentLocalIdGenerator.Object,
                Container);

            //Act
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            await handler.Handle(
                new RealizeAndMeasureUnplannedBuildingLambdaRequest(
                    buildingPersistentLocalId,
                    new RealizeAndMeasureUnplannedBuildingSqsRequest
                    {
                        BuildingPersistentLocalId = buildingPersistentLocalId,
                        IfMatchHeaderValue = null,
                        Metadata = new Dictionary<string, object?>(),
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        TicketId = Guid.NewGuid(),
                        Request = new RealizeAndMeasureUnplannedBuildingRequest
                        {
                            GrbData = new GrbData
                            {
                                GrbObjectType = "1",
                                VersionDate = SystemClock.Instance.GetCurrentInstant().ToString(),
                                GeometriePolygoon =
                                    "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                            },
                        }
                    }),
                CancellationToken.None);

            // Assert
            idempotentCommandHandler.Verify(x => x.Dispatch(
                It.IsAny<Guid>(),
                It.IsAny<RealizeUnplannedBuildingUnit>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task MainBuildingWithAlreadyExistingAddressBuildingUnitRelation_ThenNotRealizeUnplannedBuildingUnit()
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var buildings = new Mock<IBuildings>();
            buildings
                .Setup(x => x.GetAsync(It.IsAny<BuildingStreamId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Building(NoSnapshotStrategy.Instance));

            var singleUnderlyingParcelAddress = new AddressData(new AddressPersistentLocalId(123), AddressStatus.Current, false);

            var parcelMatching = new Mock<IParcelMatching>();
            parcelMatching
                .Setup(x => x.GetUnderlyingParcels(It.IsAny<Geometry>()))
                .ReturnsAsync(new List<ParcelData>
                {
                    new ParcelData(
                        Guid.NewGuid(),
                        string.Empty,
                        GeometryHelper.ValidPolygon,
                        string.Empty,
                        new List<AddressPersistentLocalId>
                        {
                            singleUnderlyingParcelAddress.AddressPersistentLocalId
                        })
                });

            var addresses = new Mock<IAddresses>();
            addresses.Setup(x => x.GetAddresses(new List<AddressPersistentLocalId>
                {
                    singleUnderlyingParcelAddress.AddressPersistentLocalId
                }))
                .ReturnsAsync(new List<AddressData> { singleUnderlyingParcelAddress });

            await _backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                new BuildingPersistentLocalId(1),
                new BuildingUnitPersistentLocalId(1),
                singleUnderlyingParcelAddress.AddressPersistentLocalId,
                CancellationToken.None);
            await _backOfficeContext.SaveChangesAsync();

            var handler = new RealizeAndMeasureUnplannedBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler.Object,
                buildings.Object,
                parcelMatching.Object,
                addresses.Object,
                _backOfficeContext,
                _persistentLocalIdGenerator.Object,
                Container);

            //Act
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            await handler.Handle(
                new RealizeAndMeasureUnplannedBuildingLambdaRequest(
                    buildingPersistentLocalId,
                    new RealizeAndMeasureUnplannedBuildingSqsRequest
                    {
                        BuildingPersistentLocalId = buildingPersistentLocalId,
                        IfMatchHeaderValue = null,
                        Metadata = new Dictionary<string, object?>(),
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        TicketId = Guid.NewGuid(),
                        Request = new RealizeAndMeasureUnplannedBuildingRequest
                        {
                            GrbData = new GrbData
                            {
                                GrbObjectType = "1",
                                VersionDate = SystemClock.Instance.GetCurrentInstant().ToString(),
                                GeometriePolygoon =
                                    "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                            },
                        }
                    }),
                CancellationToken.None);

            // Assert
            idempotentCommandHandler.Verify(x => x.Dispatch(
                It.IsAny<Guid>(),
                It.IsAny<RealizeUnplannedBuildingUnit>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task MainBuildingWithTwoUnderlyingParcelAddress_ThenNotRealizeUnplannedBuildingUnit()
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var buildings = new Mock<IBuildings>();
            buildings
                .Setup(x => x.GetAsync(It.IsAny<BuildingStreamId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Building(NoSnapshotStrategy.Instance));

            var firstUnderlyingParcelAddress = new AddressData(new AddressPersistentLocalId(123), AddressStatus.Current, false);
            var secondUnderlyingParcelAddress = new AddressData(new AddressPersistentLocalId(123), AddressStatus.Current, false);

            var parcelMatching = new Mock<IParcelMatching>();
            parcelMatching
                .Setup(x => x.GetUnderlyingParcels(It.IsAny<Geometry>()))
                .ReturnsAsync(new List<ParcelData>
                {
                    new ParcelData(
                        Guid.NewGuid(),
                        string.Empty,
                        GeometryHelper.ValidPolygon,
                        string.Empty,
                        new List<AddressPersistentLocalId>
                        {
                            firstUnderlyingParcelAddress.AddressPersistentLocalId
                        }),
                    new ParcelData(
                        Guid.NewGuid(),
                        string.Empty,
                        GeometryHelper.ValidPolygon,
                        string.Empty,
                        new List<AddressPersistentLocalId>
                        {
                            secondUnderlyingParcelAddress.AddressPersistentLocalId
                        })
                });

            var addresses = new Mock<IAddresses>();
            addresses.Setup(x => x.GetAddresses(new List<AddressPersistentLocalId>
                {
                    firstUnderlyingParcelAddress.AddressPersistentLocalId,
                    secondUnderlyingParcelAddress.AddressPersistentLocalId
                }))
                .ReturnsAsync(new List<AddressData>() { firstUnderlyingParcelAddress, secondUnderlyingParcelAddress });

            var handler = new RealizeAndMeasureUnplannedBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler.Object,
                buildings.Object,
                parcelMatching.Object,
                addresses.Object,
                _backOfficeContext,
                _persistentLocalIdGenerator.Object,
                Container);

            //Act
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            await handler.Handle(
                new RealizeAndMeasureUnplannedBuildingLambdaRequest(
                    buildingPersistentLocalId,
                    new RealizeAndMeasureUnplannedBuildingSqsRequest
                    {
                        BuildingPersistentLocalId = buildingPersistentLocalId,
                        IfMatchHeaderValue = null,
                        Metadata = new Dictionary<string, object?>(),
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        TicketId = Guid.NewGuid(),
                        Request = new RealizeAndMeasureUnplannedBuildingRequest
                        {
                            GrbData = new GrbData
                            {
                                GrbObjectType = "1",
                                VersionDate = SystemClock.Instance.GetCurrentInstant().ToString(),
                                GeometriePolygoon =
                                    "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                            },
                        }
                    }),
                CancellationToken.None);

            // Assert
            idempotentCommandHandler.Verify(x => x.Dispatch(
                It.IsAny<Guid>(),
                It.IsAny<RealizeUnplannedBuildingUnit>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task MainBuildingWithNoUnderlyingParcelAddress_ThenNotRealizeUnplannedBuildingUnit()
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var buildings = new Mock<IBuildings>();
            buildings
                .Setup(x => x.GetAsync(It.IsAny<BuildingStreamId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Building(NoSnapshotStrategy.Instance));

            var singleUnderlyingParcelAddress = new AddressData(new AddressPersistentLocalId(123), AddressStatus.Current, false);

            var parcelMatching = new Mock<IParcelMatching>();
            parcelMatching
                .Setup(x => x.GetUnderlyingParcels(It.IsAny<Geometry>()))
                .ReturnsAsync(new List<ParcelData>
                {
                    new ParcelData(
                        Guid.NewGuid(),
                        string.Empty,
                        GeometryHelper.ValidPolygon,
                        string.Empty,
                        new List<AddressPersistentLocalId>
                        {
                            singleUnderlyingParcelAddress.AddressPersistentLocalId
                        })
                });

            var addresses = new Mock<IAddresses>();
            addresses.Setup(x => x.GetAddresses(new List<AddressPersistentLocalId>
                {
                    singleUnderlyingParcelAddress.AddressPersistentLocalId
                }))
                .ReturnsAsync(new List<AddressData>());

            var handler = new RealizeAndMeasureUnplannedBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler.Object,
                buildings.Object,
                parcelMatching.Object,
                addresses.Object,
                _backOfficeContext,
                _persistentLocalIdGenerator.Object,
                Container);

            //Act
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            await handler.Handle(
                new RealizeAndMeasureUnplannedBuildingLambdaRequest(
                    buildingPersistentLocalId,
                    new RealizeAndMeasureUnplannedBuildingSqsRequest
                    {
                        BuildingPersistentLocalId = buildingPersistentLocalId,
                        IfMatchHeaderValue = null,
                        Metadata = new Dictionary<string, object?>(),
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        TicketId = Guid.NewGuid(),
                        Request = new RealizeAndMeasureUnplannedBuildingRequest
                        {
                            GrbData = new GrbData
                            {
                                GrbObjectType = "1",
                                VersionDate = SystemClock.Instance.GetCurrentInstant().ToString(),
                                GeometriePolygoon =
                                    "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                            },
                        }
                    }),
                CancellationToken.None);

            // Assert
            idempotentCommandHandler.Verify(x => x.Dispatch(
                It.IsAny<Guid>(),
                It.IsAny<RealizeUnplannedBuildingUnit>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task MainBuildingWithOneUnderlyingParcelAddress_ThenRealizeUnplannedBuildingUnit()
        {
            var idempotentCommandHandler = new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext);
            var buildings = new Mock<IBuildings>();
            buildings
                .Setup(x => x.GetAsync(It.IsAny<BuildingStreamId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Building(NoSnapshotStrategy.Instance));

            var singleUnderlyingParcelAddress = new AddressData(new AddressPersistentLocalId(123), AddressStatus.Current, false);

            var parcelMatching = new Mock<IParcelMatching>();
            parcelMatching
                .Setup(x => x.GetUnderlyingParcels(It.IsAny<Geometry>()))
                .ReturnsAsync(new List<ParcelData>
                {
                    new ParcelData(
                        Guid.NewGuid(),
                        string.Empty,
                        GeometryHelper.ValidPolygon,
                        string.Empty,
                        new List<AddressPersistentLocalId>
                        {
                            singleUnderlyingParcelAddress.AddressPersistentLocalId
                        })
                });

            var addresses = new Mock<IAddresses>();
            addresses.Setup(x => x.GetAddresses(new List<AddressPersistentLocalId>
                {
                    singleUnderlyingParcelAddress.AddressPersistentLocalId
                }))
                .ReturnsAsync(new List<AddressData> { singleUnderlyingParcelAddress });

            var handler = new RealizeAndMeasureUnplannedBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler,
                buildings.Object,
                parcelMatching.Object,
                addresses.Object,
                _backOfficeContext,
                _persistentLocalIdGenerator.Object,
                Container);

            //Act
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            await handler.Handle(
                new RealizeAndMeasureUnplannedBuildingLambdaRequest(
                    buildingPersistentLocalId,
                    new RealizeAndMeasureUnplannedBuildingSqsRequest
                    {
                        BuildingPersistentLocalId = buildingPersistentLocalId,
                        IfMatchHeaderValue = null,
                        Metadata = new Dictionary<string, object?>(),
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        TicketId = Guid.NewGuid(),
                        Request = new RealizeAndMeasureUnplannedBuildingRequest
                        {
                            GrbData = new GrbData
                            {
                                GrbObjectType = "1",
                                VersionDate = SystemClock.Instance.GetCurrentInstant().ToString(),
                                GeometriePolygoon =
                                    "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                            },
                        }
                    }),
                CancellationToken.None);

            // Assert
            var stream = await Container.Resolve<IStreamStore>().ReadStreamBackwards(new StreamId(new BuildingStreamId(buildingPersistentLocalId)), 4, 5);
            stream.Messages.Should().HaveCount(5);
            stream.Messages.First().Type.Should().Be(nameof(BuildingUnitAddressWasAttachedV2));
        }

        [Fact]
        public async Task ThenRealizeUnplannedBuildingUnitIsIdempotent()
        {
            var firstBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(1);
            var secondBuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(2);
            _persistentLocalIdGenerator
                .SetupSequence(x => x.GenerateNextPersistentLocalId())
                .Returns(firstBuildingUnitPersistentLocalId)
                .Returns(secondBuildingUnitPersistentLocalId);

            var singleUnderlyingParcelAddress = new AddressData(new AddressPersistentLocalId(123), AddressStatus.Current, false);

            var parcelMatching = new Mock<IParcelMatching>();
            parcelMatching
                .Setup(x => x.GetUnderlyingParcels(It.IsAny<Geometry>()))
                .ReturnsAsync(new List<ParcelData>
                {
                    new ParcelData(
                        Guid.NewGuid(),
                        string.Empty,
                        GeometryHelper.ValidPolygon,
                        string.Empty,
                        new List<AddressPersistentLocalId>
                        {
                            singleUnderlyingParcelAddress.AddressPersistentLocalId
                        })
                });

            var addresses = new Mock<IAddresses>();
            addresses
                .Setup(x => x.GetAddresses(new List<AddressPersistentLocalId> { singleUnderlyingParcelAddress.AddressPersistentLocalId }))
                .ReturnsAsync(new List<AddressData> { singleUnderlyingParcelAddress });

            var idempotentCommandHandler = new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext);
            var handler = new RealizeAndMeasureUnplannedBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler,
                Container.Resolve<IBuildings>(),
                parcelMatching.Object,
                addresses.Object,
                _backOfficeContext,
                _persistentLocalIdGenerator.Object,
                Container);

            //Act
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var provenanceData = Fixture.Create<ProvenanceData>();

            var request = new RealizeAndMeasureUnplannedBuildingLambdaRequest(
                buildingPersistentLocalId,
                new RealizeAndMeasureUnplannedBuildingSqsRequest
                {
                    BuildingPersistentLocalId = buildingPersistentLocalId,
                    IfMatchHeaderValue = null,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = provenanceData,
                    TicketId = Guid.NewGuid(),
                    Request = new RealizeAndMeasureUnplannedBuildingRequest
                    {
                        GrbData = new GrbData
                        {
                            GrbObjectType = "1",
                            VersionDate = SystemClock.Instance.GetCurrentInstant().ToString(),
                            GeometriePolygoon =
                                "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                        },
                    }
                });

            await handler.Handle(
                request,
                CancellationToken.None);
            await handler.Handle(
                request,
                CancellationToken.None);

            // Assert
            var buildingRelationOnFirstBuildingUnitPersistentLocalid = await _backOfficeContext.FindBuildingUnitBuildingRelation(
                firstBuildingUnitPersistentLocalId,
                CancellationToken.None);
            buildingRelationOnFirstBuildingUnitPersistentLocalid.Should().NotBeNull();

            var addressRelationOnFirstBuildingUnitPersistentLocalid = await _backOfficeContext.FindBuildingUnitAddressRelation(
                firstBuildingUnitPersistentLocalId,
                singleUnderlyingParcelAddress.AddressPersistentLocalId,
                CancellationToken.None);
            addressRelationOnFirstBuildingUnitPersistentLocalid.Should().NotBeNull();

            var addressRelationOnSecondBuildingUnitPersistentLocalid = await _backOfficeContext.FindBuildingUnitAddressRelation(
                secondBuildingUnitPersistentLocalId,
                singleUnderlyingParcelAddress.AddressPersistentLocalId,
                CancellationToken.None);
            addressRelationOnSecondBuildingUnitPersistentLocalid.Should().BeNull();

            var buildingRelationOnSecondBuildingUnitPersistentLocalid = await _backOfficeContext.FindBuildingUnitBuildingRelation(
                secondBuildingUnitPersistentLocalId,
                CancellationToken.None);
            buildingRelationOnSecondBuildingUnitPersistentLocalid.Should().BeNull();
        }

        [Theory]
        [InlineData("0")]
        [InlineData("2")]
        [InlineData("3")]
        public async Task NotMainBuilding_ThenNoRealizeUnplannedBuildingUnitCommand(string grbObjectType)
        {
            // Arrange
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();
            var buildings = new Mock<IBuildings>();
            buildings
                .Setup(x => x.GetAsync(It.IsAny<BuildingStreamId>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Building(NoSnapshotStrategy.Instance));

            var handler = new RealizeAndMeasureUnplannedBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler.Object,
                buildings.Object,
                Mock.Of<IParcelMatching>(),
                Mock.Of<IAddresses>(),
                _backOfficeContext,
                _persistentLocalIdGenerator.Object,
                Container);

            //Act
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            await handler.Handle(
                new RealizeAndMeasureUnplannedBuildingLambdaRequest(
                    buildingPersistentLocalId,
                    new RealizeAndMeasureUnplannedBuildingSqsRequest
                    {
                        BuildingPersistentLocalId = buildingPersistentLocalId,
                        IfMatchHeaderValue = null,
                        Metadata = new Dictionary<string, object?>(),
                        ProvenanceData = Fixture.Create<ProvenanceData>(),
                        TicketId = Guid.NewGuid(),
                        Request = new RealizeAndMeasureUnplannedBuildingRequest
                        {
                            GrbData = new GrbData
                            {
                                GrbObjectType = grbObjectType,
                                VersionDate = SystemClock.Instance.GetCurrentInstant().ToString(),
                                GeometriePolygoon =
                                    "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                            },
                        }
                    }),
                CancellationToken.None);

            //Assert
            idempotentCommandHandler.Verify(x => x.Dispatch(
                It.IsAny<Guid?>(),
                It.IsAny<RealizeUnplannedBuildingUnit>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
