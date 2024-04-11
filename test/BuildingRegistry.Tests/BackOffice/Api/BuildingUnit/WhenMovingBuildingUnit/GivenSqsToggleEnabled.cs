namespace BuildingRegistry.Tests.BackOffice.Api.BuildingUnit.WhenMovingBuildingUnit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Building;
    using Fixtures;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using SqlStreamStore;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSqsToggleEnabled : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;
        private readonly Mock<IStreamStore> _streamStore;

        public GivenSqsToggleEnabled(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _streamStore = new Mock<IStreamStore>();
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<MoveBuildingUnitSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            _streamStore.SetStreamFound();

            var request = Fixture.Create<MoveBuildingUnitRequest>();
            var expectedIfMatchHeader = Fixture.Create<string>();

            var result = (AcceptedResult)await _controller.Move(
                MockIfMatchValidator(true),
                MockValidRequestValidator<MoveBuildingUnitExtendedRequest>(),
                0,
                request,
                expectedIfMatchHeader,
                CancellationToken.None);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<MoveBuildingUnitSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.BuildingRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Update
                        && sqsRequest.IfMatchHeaderValue == expectedIfMatchHeader
                    ),
                    CancellationToken.None));
        }

        [Fact]
        public void WithInvalidRequest_ThenValidationExceptionIsThrown()
        {
            var request = new MoveBuildingUnitRequest
            {
                DoelgebouwId = ""
            };

            var streamStoreMock = new Mock<IStreamStore>();
            streamStoreMock.SetStreamFound();

            //Act
            Func<Task> act = async () => await _controller.Move(
                MockIfMatchValidator(true),
                new MoveBuildingUnitExtendedRequestValidator(
                    new BuildingExistsValidator(streamStoreMock.Object),
                    new FakeBackOfficeContextFactory().CreateDbContext([])),
                0,
                request,
                null,
                CancellationToken.None);

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>();
        }

        [Fact]
        public void WithAggregateIdNotFound_ThenValidationErrorIsThrown()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<MoveBuildingUnitSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            _streamStore.SetStreamNotFound();

            var request = Fixture.Create<MoveBuildingUnitRequest>();

            var act = async () =>
            {
                await _controller.Move(

                    MockIfMatchValidator(true),
                    MockValidRequestValidator<MoveBuildingUnitExtendedRequest>(),
                    0,
                    request,
                    string.Empty);
            };

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Where(x =>
                    x.Message.Contains("Onbestaande gebouweenheid.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public void WhenAggregateNotFoundException_ThenThrowValidationException()
        {
            MockMediator.Setup<object?>(x => x.Send(It.IsAny<MoveBuildingUnitSqsRequest>(), CancellationToken.None).Result)
                .Throws(new AggregateNotFoundException("", typeof(Building)));

            var request = Fixture.Create<MoveBuildingUnitRequest>();

            //Act
            Func<Task> act = async () => await _controller.Move(
                MockIfMatchValidator(true),
                MockValidRequestValidator<MoveBuildingUnitExtendedRequest>(),
                0,
                request,
                null,
                CancellationToken.None);

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Where(x =>
                    x.Message.Contains("Onbestaande gebouweenheid.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }
    }
}
