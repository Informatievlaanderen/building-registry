namespace BuildingRegistry.Tests.BackOffice.Api.BuildingUnit.WhenAttachingAddressToBuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit;
    using BuildingRegistry.Building;
    using Fixtures;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
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
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<AttachAddressToBuildingUnitSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var result = (AcceptedResult)await _controller.AttachAddress(
                MockIfMatchValidator(true),
                MockValidRequestValidator<AttachAddressToBuildingUnitRequest>(),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                Fixture.Create<AttachAddressToBuildingUnitRequest>(),
                ifMatchHeaderValue: null);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public void GivenInvalidAddress_ThenThrowValidationException()
        {
            //Act
            Func<Task> act = async () =>
            {
                await _controller.AttachAddress(
                    MockIfMatchValidator(true),
                    new AttachAddressToBuildingUnitRequestValidator(Mock.Of<IAddresses>()),
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    new AttachAddressToBuildingUnitRequest { AdresId = "https://invalid.vlaanderen.be/notAnId/notAnAddress/xyz" },
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
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            //Act
            var result = await _controller.AttachAddress(
                MockIfMatchValidator(false),
                MockValidRequestValidator<AttachAddressToBuildingUnitRequest>(),
                Fixture.Create<BuildingUnitPersistentLocalId>(),
                Fixture.Create<AttachAddressToBuildingUnitRequest>(),
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }

        [Fact]
        public void WhenAggregateNotFoundException_ThenThrowValidationException()
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<AttachAddressToBuildingUnitSqsRequest>(), CancellationToken.None).Result)
                .Throws(new AggregateNotFoundException(buildingPersistentLocalId, typeof(Building)));

            //Act
            Func<Task> act = async () =>
            {
                await _controller.AttachAddress(
                    MockIfMatchValidator(true),
                    MockValidRequestValidator<AttachAddressToBuildingUnitRequest>(),
                    buildingPersistentLocalId,
                    new AttachAddressToBuildingUnitRequest(),
                    null,
                    CancellationToken.None);
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
        public void WithAggregateIdIsNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<AttachAddressToBuildingUnitSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            var request = Fixture.Create<AttachAddressToBuildingUnitRequest>();
            Func<Task> act = async () =>
            {
                await _controller.AttachAddress(
                    MockIfMatchValidator(true),
                    MockValidRequestValidator<AttachAddressToBuildingUnitRequest>(),
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
    }
}
