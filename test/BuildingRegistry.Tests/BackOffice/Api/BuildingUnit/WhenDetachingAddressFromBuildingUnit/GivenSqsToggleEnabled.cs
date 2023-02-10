namespace BuildingRegistry.Tests.BackOffice.Api.BuildingUnit.WhenDetachingAddressFromBuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using Fixtures;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NodaTime;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSqsToggleEnabled : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenSqsToggleEnabled(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            //Act
            Func<Task> act = async () =>
            {
                await _controller.DetachAddress(
                    MockIfMatchValidator(true),
                    new DetachAddressFromBuildingUnitRequestValidator(Mock.Of<IAddresses>()),
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    new DetachAddressFromBuildingUnitRequest {AdresId = "https://invalid.vlaanderen.be/notAnId/notAnAddress/xyz" },
                    null,
                    CancellationToken.None);
            };

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "GebouweenheidAdresOngeldig"
                    && e.ErrorMessage == "Ongeldig adresId."));
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<DetachAddressFromBuildingUnitSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var request = Fixture.Create<DetachAddressFromBuildingUnitRequest>();
            var expectedIfMatchHeader = Fixture.Create<string>();

            var result = (AcceptedResult)await _controller.DetachAddress(
                MockIfMatchValidator(true),
                MockValidRequestValidator<DetachAddressFromBuildingUnitRequest>(),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                request,
                expectedIfMatchHeader);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);

            MockMediator.Verify(x =>
                x.Send(
                    It.Is<DetachAddressFromBuildingUnitSqsRequest>(sqsRequest =>
                        sqsRequest.Request == request
                        && sqsRequest.ProvenanceData.Timestamp != Instant.MinValue
                        && sqsRequest.ProvenanceData.Application == Application.BuildingRegistry
                        && sqsRequest.ProvenanceData.Modification == Modification.Update
                        && sqsRequest.IfMatchHeaderValue == expectedIfMatchHeader
                    ),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            //Act
            var result = await _controller.DetachAddress(

                MockIfMatchValidator(false),
                MockValidRequestValidator<DetachAddressFromBuildingUnitRequest>(),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                Fixture.Create<DetachAddressFromBuildingUnitRequest>(),
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public void WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<DetachAddressFromBuildingUnitSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            var request = Fixture.Create<DetachAddressFromBuildingUnitRequest>();
            Func<Task> act = async () =>
            {
                await _controller.DetachAddress(

                    MockIfMatchValidator(true),
                    MockValidRequestValidator<DetachAddressFromBuildingUnitRequest>(),
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    request,
                    string.Empty);
            };

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaande gebouweenheid.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public void WithAggregateNotFoundException_ThenThrowsApiException()
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<DetachAddressFromBuildingUnitSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateNotFoundException(buildingPersistentLocalId, typeof(Building)));

            var request = Fixture.Create<DetachAddressFromBuildingUnitRequest>();

            Func<Task> act = async () =>
            {
                await _controller.DetachAddress(
                    MockIfMatchValidator(true),
                    MockValidRequestValidator<DetachAddressFromBuildingUnitRequest>(),
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    request,
                    string.Empty);
            };

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaande gebouweenheid.")
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }


        [Fact]
        public void WhenBuildingUnitNotFound_ThenThrowValidationException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<DetachAddressFromBuildingUnitSqsRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitIsNotFoundException());

            //Act
            Func<Task> act = async () =>
            {
                await _controller.DetachAddress(
                    MockIfMatchValidator(true),
                    MockValidRequestValidator<DetachAddressFromBuildingUnitRequest>(),
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    new DetachAddressFromBuildingUnitRequest(),
                    null,
                    CancellationToken.None);
            };

            // Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message == "Onbestaande gebouweenheid."
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

    }
}
