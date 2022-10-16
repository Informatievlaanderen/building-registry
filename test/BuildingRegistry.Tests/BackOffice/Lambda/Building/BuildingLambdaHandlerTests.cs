namespace BuildingRegistry.Tests.BackOffice.Lambda.Building
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using BuildingRegistry.Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class SqsLambdaBuildingHandlerTests : BackOfficeLambdaTest
    {
        public SqsLambdaBuildingHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public async Task TicketShouldBeUpdatedToPendingAndCompleted()
        {
            var ticketing = new Mock<ITicketing>();
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var lambdaRequest = new RealizeBuildingLambdaRequest(
                Guid.NewGuid().ToString(),
                Guid.NewGuid(),
                null,
                Fixture.Create<Provenance>(),
                new Dictionary<string, object?>(),
                new RealizeBuildingBackOfficeRequest { PersistentLocalId = 1 }
            );

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

            var lambdaRequest = new RealizeBuildingLambdaRequest(
                Guid.NewGuid().ToString(),
                Guid.NewGuid(),
                null,
                Fixture.Create<Provenance>(),
                new Dictionary<string, object?>(),
                new RealizeBuildingBackOfficeRequest { PersistentLocalId = 1 }
            );

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
                    x.Error(lambdaRequest.TicketId, new TicketError("Verwijderd gebouw.", "GebouwIsVerwijderd"),
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
                new RealizeBuildingLambdaRequest(
                    buildingPersistentLocalId.ToString(),
                    Guid.Empty,
                    "Outdated",
                    Fixture.Create<Provenance>(),
                    new Dictionary<string, object?>(),
                    new RealizeBuildingBackOfficeRequest
                    {
                        PersistentLocalId = buildingPersistentLocalId
                    }),
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
                new RealizeBuildingLambdaRequest(
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid(),
                    string.Empty,
                    Fixture.Create<Provenance>(),
                    new Dictionary<string, object?>(),
                    new RealizeBuildingBackOfficeRequest { PersistentLocalId = 1 }),
                CancellationToken.None);

            //Assert
            idempotentCommandHandler
                .Verify(
                    x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>(), new CancellationToken()),
                    Times.Once);
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
        {
        }

        protected override Task<ETagResponse> InnerHandle(
            RealizeBuildingLambdaRequest request,
            CancellationToken cancellationToken)
        {
            IdempotentCommandHandler.Dispatch(
                Guid.NewGuid(),
                new object(),
                new Dictionary<string, object>(),
                cancellationToken);

            return Task.FromResult(new ETagResponse("location", "etag"));
        }

        protected override TicketError MapDomainException(DomainException exception,
            RealizeBuildingLambdaRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
