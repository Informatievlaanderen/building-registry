namespace BuildingRegistry.Tests.BackOffice.Lambda.BuildingUnit
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
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Handlers.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Fixtures;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class BuildingUnitLambdaHandlerTests : BackOfficeLambdaTest
    {
        public BuildingUnitLambdaHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingUnitPersistentLocalId());
        }

        [Fact]
        public async Task ThenTicketIsUpdatedToPendingAndCompleted()
        {
            var ticketing = new Mock<ITicketing>();
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var lambdaRequest = CreateRealizeBuildingUnitLambdaRequest();

            var sut = new FakeBuildingUnitLambdaHandler(
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
        public async Task WithRemovedBuilding_ThenTicketingErrorIsExpected()
        {
            var ticketing = new Mock<ITicketing>();

            var lambdaRequest = CreateRealizeBuildingUnitLambdaRequest();

            var sut = new FakeBuildingUnitLambdaHandler(
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
        public async Task WithRemovedBuildingUnit_ThenTicketingErrorIsExpected()
        {
            var ticketing = new Mock<ITicketing>();

            var lambdaRequest = CreateRealizeBuildingUnitLambdaRequest();

            var sut = new FakeBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IBuildings>(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingUnitIsRemovedException>().Object);

            await sut.Handle(lambdaRequest, CancellationToken.None);

            //Assert
            ticketing
                .Verify(x =>
                    x.Error(lambdaRequest.TicketId, new TicketError("Verwijderde gebouweenheid.", "VerwijderdeGebouweenheid"),
                        CancellationToken.None));
            ticketing
                .Verify(x =>
                    x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task WithNotFoundBuildingUnit_ThenTicketingErrorIsExpected()
        {
            var ticketing = new Mock<ITicketing>();

            var lambdaRequest = CreateRealizeBuildingUnitLambdaRequest();

            var sut = new FakeBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IBuildings>(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<BuildingUnitIsNotFoundException>().Object);

            await sut.Handle(lambdaRequest, CancellationToken.None);

            //Assert
            ticketing
                .Verify(x =>
                    x.Error(lambdaRequest.TicketId, new TicketError("Onbestaande gebouweenheid.", "GebouweenheidNietGevonden"),
                        CancellationToken.None));
            ticketing
                .Verify(x =>
                    x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None), Times.Never);
        }

        [Fact]
        public async Task WithMisMatchingIfHeaderValue_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            PlanBuilding(buildingPersistentLocalId);
            PlanBuildingUnit(buildingPersistentLocalId, buildingUnitPersistentLocalId);

            var sut = new FakeBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Container.Resolve<IBuildings>(),
                ticketing.Object,
                Mock.Of<IIdempotentCommandHandler>());

            // Act
            await sut.Handle(CreateRealizeBuildingUnitLambdaRequest("Outdated"), CancellationToken.None);


            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError("Als de If-Match header niet overeenkomt met de laatste ETag.", "PreconditionFailed"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WithoutIfMatchHeaderValue_ThenInnerHandleIsCalled()
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var sut = new FakeBuildingUnitLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IBuildings>(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler.Object);

            await sut.Handle(CreateRealizeBuildingUnitLambdaRequest(), CancellationToken.None);

            //Assert
            idempotentCommandHandler
                .Verify(
                    x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>(), new CancellationToken()),
                    Times.Once);
        }

        private RealizeBuildingUnitLambdaRequest CreateRealizeBuildingUnitLambdaRequest(string? ifMatchHeaderValue = null)
        {
            return new RealizeBuildingUnitLambdaRequest(Fixture.Create<BuildingPersistentLocalId>(),
                new RealizeBuildingUnitSqsRequest()
                {
                    IfMatchHeaderValue = ifMatchHeaderValue ?? string.Empty,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = Fixture.Create<ProvenanceData>(),
                    Request = new RealizeBuildingUnitRequest { BuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>() },
                    TicketId = Guid.NewGuid()
                });
        }
    }

    public sealed class FakeBuildingUnitLambdaHandler : BuildingUnitLambdaHandler<RealizeBuildingUnitLambdaRequest>
    {
        public FakeBuildingUnitLambdaHandler(
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
            RealizeBuildingUnitLambdaRequest request,
            CancellationToken cancellationToken)
        {
            IdempotentCommandHandler.Dispatch(
                Guid.NewGuid(),
                new object(),
                new Dictionary<string, object>(),
                cancellationToken);

            return Task.FromResult(new ETagResponse("location", "etag"));
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, RealizeBuildingUnitLambdaRequest request)
        {
            return null;
        }
    }
}
