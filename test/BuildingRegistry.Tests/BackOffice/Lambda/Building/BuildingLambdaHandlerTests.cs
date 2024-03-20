namespace BuildingRegistry.Tests.BackOffice.Lambda.Building
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Fixtures;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class SqsLambdaBuildingHandlerTests : BackOfficeLambdaTest
    {
        public SqsLambdaBuildingHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public async Task TicketShouldBeUpdatedToPendingAndCompleted()
        {
            var ticketing = new Mock<ITicketing>();
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var lambdaRequest = CreateRealizeBuildingLambdaRequest();

            var sut = new FakeBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IBuildings>(),
                ticketing.Object,
                idempotentCommandHandler.Object);

            await sut.Handle(lambdaRequest, CancellationToken.None);

            ticketing.Verify(x => x.Pending(lambdaRequest.TicketId, CancellationToken.None), Times.Once);
            ticketing.Verify(
                x => x.Complete(lambdaRequest.TicketId,
                    new TicketResult(new ETagResponse("location", "etag")), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenBuildingIsRemovedException_ThenTicketingErrorIsExpected()
        {
            var ticketing = new Mock<ITicketing>();

            var lambdaRequest = CreateRealizeBuildingLambdaRequest();

            var sut = new FakeBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IBuildings>(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingIsRemovedException>().Object);

            await sut.Handle(lambdaRequest, CancellationToken.None);

            //Assert
            ticketing
                .Verify(x =>
                    x.Error(lambdaRequest.TicketId, new TicketError("Verwijderd gebouw.", "VerwijderdGebouw"),
                        CancellationToken.None));
            ticketing
                .Verify(x =>
                    x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task WhenIfMatchHeaderValueIsMismatch_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);

            var sut = new FakeBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Container.Resolve<IBuildings>(),
                ticketing.Object,
                Mock.Of<IIdempotentCommandHandler>());

            // Act
            await sut.Handle(
                CreateRealizeBuildingLambdaRequest("Outdated"),
                CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError("Als de If-Match header niet overeenkomt met de laatste ETag.",
                        "PreconditionFailed"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenNoIfMatchHeaderValueIsPresent_ThenInnerHandleIsCalled()
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var sut = new FakeBuildingLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IBuildings>(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler.Object);

            await sut.Handle(
                CreateRealizeBuildingLambdaRequest(),
                CancellationToken.None);

            //Assert
            idempotentCommandHandler
                .Verify(
                    x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>(), new CancellationToken()),
                    Times.Once);
        }

        private RealizeBuildingLambdaRequest CreateRealizeBuildingLambdaRequest(string? ifMatchHeaderValue = null)
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            return new RealizeBuildingLambdaRequest(buildingPersistentLocalId, new RealizeBuildingSqsRequest()
            {
                IfMatchHeaderValue = ifMatchHeaderValue,
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>(),
                Request = new RealizeBuildingRequest { PersistentLocalId = buildingPersistentLocalId },
                TicketId = Guid.NewGuid()
            });
        }
    }

    public sealed class FakeBuildingLambdaHandler : BuildingLambdaHandler<RealizeBuildingLambdaRequest>
    {
        public FakeBuildingLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            IBuildings buildings,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler)
            : base(
                configuration,
                retryPolicy,
                ticketing,
                idempotentCommandHandler,
                buildings)
        { }

        protected override Task<object> InnerHandle(
            RealizeBuildingLambdaRequest request,
            CancellationToken cancellationToken)
        {
            IdempotentCommandHandler.Dispatch(
                Guid.NewGuid(),
                new object(),
                new Dictionary<string, object>(),
                cancellationToken);

            return Task.FromResult((object)new ETagResponse("location", "etag"));
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, RealizeBuildingLambdaRequest request)
        {
            return null;
        }
    }
}
