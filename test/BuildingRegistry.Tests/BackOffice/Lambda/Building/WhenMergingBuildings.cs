namespace BuildingRegistry.Tests.BackOffice.Lambda.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Consumer.Address;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using SqlStreamStore;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class WhenMergingBuildings : BackOfficeLambdaTest
    {
        private readonly BackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;
        private readonly FakeConsumerAddressContext _addressConsumerContext;

        private readonly CancellationToken _ct = CancellationToken.None;

        public WhenMergingBuildings(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _backOfficeContext = Container.Resolve<BackOfficeContext>();
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
            _addressConsumerContext = Container.Resolve<FakeConsumerAddressContext>();
        }

        [Fact]
        public async Task ThenBackOfficeRelationsAreReCoupled()
        {
            var newBuildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            // BUILDING 1
            var buildingToMerge1 = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitToTransfer1 = Fixture.Create<BuildingUnitPersistentLocalId>();
            var buildingUnitToTransfer1_Address1 = Fixture.Create<AddressPersistentLocalId>();
            var buildingUnitToTransfer1_Address2 = Fixture.Create<AddressPersistentLocalId>();

            PlanBuilding(buildingToMerge1);
            MeasureBuilding(buildingToMerge1, Fixture.Create<ExtendedWkbGeometry>());
            PlanBuildingUnit(buildingToMerge1, buildingUnitToTransfer1);
            await _backOfficeContext.AddIdempotentBuildingUnitBuilding(buildingToMerge1, buildingUnitToTransfer1, _ct);
            _addressConsumerContext.AddAddress(buildingUnitToTransfer1_Address1, AddressStatus.Current);
            _addressConsumerContext.AddAddress(buildingUnitToTransfer1_Address2, AddressStatus.Current);
            AttachAddressToBuildingUnit(buildingToMerge1, buildingUnitToTransfer1, buildingUnitToTransfer1_Address1);
            AttachAddressToBuildingUnit(buildingToMerge1, buildingUnitToTransfer1, buildingUnitToTransfer1_Address2);
            await _backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                buildingToMerge1,
                buildingUnitToTransfer1,
                buildingUnitToTransfer1_Address1, _ct);
            await _backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                buildingToMerge1,
                buildingUnitToTransfer1,
                buildingUnitToTransfer1_Address2, _ct);

            // BUILDING 2
            var buildingToMerge2 = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitToTransfer2 = Fixture.Create<BuildingUnitPersistentLocalId>();
            var buildingUnitToTransfer2_Address1 = Fixture.Create<AddressPersistentLocalId>();
            var buildingUnitToTransfer2_Address2 = Fixture.Create<AddressPersistentLocalId>();

            PlanBuilding(buildingToMerge2);
            MeasureBuilding(buildingToMerge2, Fixture.Create<ExtendedWkbGeometry>());
            PlanBuildingUnit(buildingToMerge2, buildingUnitToTransfer2);
            await _backOfficeContext.AddIdempotentBuildingUnitBuilding(buildingToMerge2, buildingUnitToTransfer2, _ct);
            _addressConsumerContext.AddAddress(buildingUnitToTransfer2_Address1, AddressStatus.Current);
            _addressConsumerContext.AddAddress(buildingUnitToTransfer2_Address2, AddressStatus.Current);
            AttachAddressToBuildingUnit(buildingToMerge2, buildingUnitToTransfer2, buildingUnitToTransfer2_Address1);
            AttachAddressToBuildingUnit(buildingToMerge2, buildingUnitToTransfer2, buildingUnitToTransfer2_Address2);
            await _backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                buildingToMerge2,
                buildingUnitToTransfer2,
                buildingUnitToTransfer2_Address1, _ct);
            await _backOfficeContext.AddIdempotentBuildingUnitAddressRelation(
                buildingToMerge2,
                buildingUnitToTransfer2,
                buildingUnitToTransfer2_Address2, _ct);

            var request = new MergeBuildingsLambdaRequest(
                newBuildingPersistentLocalId,
                new MergeBuildingsSqsRequest
                {
                    BuildingPersistentLocalId = newBuildingPersistentLocalId,
                    Request = new MergeBuildingRequest
                    {
                        GeometriePolygoon =
                            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                        SamenvoegenGebouwen = new List<string>
                        {
                            PuriCreator.CreateBuildingId(buildingToMerge1),
                            PuriCreator.CreateBuildingId(buildingToMerge2)
                        }
                    },
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>(),
                    TicketId = Guid.NewGuid()
                });

            var eTagResponse = new ETagResponse(string.Empty, Fixture.Create<string>());

            var handler = new MergeBuildingsLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                MockTicketing(response => { eTagResponse = response; }).Object,
                new IdempotentCommandHandler(Container.Resolve<ICommandHandlerResolver>(), _idempotencyContext),
                Container.Resolve<IBuildings>(),
                _backOfficeContext,
                Container.Resolve<ILifetimeScope>());

            // Act
            await handler.Handle(request, CancellationToken.None);

            // Assert
            var stream = await Container
                .Resolve<IStreamStore>()
                .ReadStreamBackwards(new BuildingStreamId(newBuildingPersistentLocalId), 3, 1);
            stream.Messages.First().JsonMetadata.Should().Contain(eTagResponse.ETag);

            _backOfficeContext.BuildingUnitBuildings.Should().HaveCount(3);

            var relation = _backOfficeContext.BuildingUnitBuildings.Find((int) buildingUnitToTransfer1);
            relation.BuildingPersistentLocalId.Should().Be(newBuildingPersistentLocalId);

            AssertBuildingUnitAddressRelation(buildingUnitToTransfer1, buildingUnitToTransfer1_Address1, newBuildingPersistentLocalId);
            AssertBuildingUnitAddressRelation(buildingUnitToTransfer1, buildingUnitToTransfer1_Address2, newBuildingPersistentLocalId);

            relation = _backOfficeContext.BuildingUnitBuildings.Find((int) buildingUnitToTransfer2);
            relation.BuildingPersistentLocalId.Should().Be(newBuildingPersistentLocalId);

            AssertBuildingUnitAddressRelation(buildingUnitToTransfer2, buildingUnitToTransfer2_Address1, newBuildingPersistentLocalId);
            AssertBuildingUnitAddressRelation(buildingUnitToTransfer2, buildingUnitToTransfer2_Address2, newBuildingPersistentLocalId);

            relation = _backOfficeContext.BuildingUnitBuildings.Find(1); // CommonBuildingUnit persistentLocalId
            relation.BuildingPersistentLocalId.Should().Be(newBuildingPersistentLocalId);
        }

        private void AssertBuildingUnitAddressRelation(BuildingUnitPersistentLocalId buildingUnitToTransfer1,
            AddressPersistentLocalId buildingUnitToTransfer1_Address1, BuildingPersistentLocalId newBuildingPersistentLocalId)
        {
            var unitAddressRelation = _backOfficeContext.BuildingUnitAddressRelation.Find(
                (int) buildingUnitToTransfer1,
                (int) buildingUnitToTransfer1_Address1);
            unitAddressRelation.Should().NotBeNull();
            unitAddressRelation.BuildingPersistentLocalId.Should().Be(newBuildingPersistentLocalId);
        }

        [Fact]
        public async Task WhenBuildingToMergeHasInvalidStatus_ThenThrowsBuildingToMergeHasInvalidStatusException()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new MergeBuildingsLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingToMergeHasInvalidStatusException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext,
                Container);

            // Act
            await handler.Handle(CreateMergeBuildingsLambdaRequest(), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan op gebouwen met status 'gerealiseerd'.",
                        "GebouwStatusGeplandInaanbouwNietgerealiseerdGehistoreerd"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenOnlyOneBuildingToMerge_ThenThrowsBuildingMergerNeedsMoreThanOneBuildingException()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new MergeBuildingsLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingMergerNeedsMoreThanOneBuildingException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext,
                Container);

            // Act
            await handler.Handle(CreateMergeBuildingsLambdaRequest(), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "TooFewBuildings",
                        "TooFewBuildings"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenMoreThanMaxBuildingsToMerge_ThenThrowsBuildingMergerHasTooManyBuildingsException()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new MergeBuildingsLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingMergerHasTooManyBuildingsException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext,
                Container);

            // Act
            await handler.Handle(CreateMergeBuildingsLambdaRequest(), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "TooManyBuildings",
                        "TooManyBuildings"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenBuildingToMergeIsNotMeasuredByGrb_ThenThrowsBuildingToMergeHasInvalidGeometryMethodException()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var handler = new MergeBuildingsLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingToMergeHasInvalidGeometryMethodException>().Object,
                Container.Resolve<IBuildings>(),
                _backOfficeContext,
                Container);

            // Act
            await handler.Handle(CreateMergeBuildingsLambdaRequest(), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Ongeldig formaat geometriePolygoon.",
                        "GebouwPolygoonValidatie"
                        ),
                    CancellationToken.None));
        }

        private MergeBuildingsLambdaRequest CreateMergeBuildingsLambdaRequest()
        {
            return new MergeBuildingsLambdaRequest(
                "123",
                new MergeBuildingsSqsRequest
                {
                    BuildingPersistentLocalId = new BuildingPersistentLocalId(123),
                    Request = new MergeBuildingRequest
                    {
                        GeometriePolygoon =
                            "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>",
                        SamenvoegenGebouwen = new List<string>()
                    },
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>(),
                    TicketId = Guid.NewGuid()
                }
            );
        }
    }
}
