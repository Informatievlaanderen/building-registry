namespace BuildingRegistry.Tests.BackOffice.Api.BuildingUnit.WhenCorrectingBuildingUnitPosition
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
    using BuildingRegistry.Tests.Fixtures;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
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

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void WithPositionGeometryMethodAppointedByAdministratorAndMissingPosition_ThenValidationExceptionIsThrown(string? position)
        {
            var request = new CorrectBuildingUnitPositionRequest
            {
                BuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456),
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = position
            };

            var streamStoreMock = new Mock<IStreamStore>();
            streamStoreMock.SetStreamFound();

            //Act
            Func<Task> act = async () => await _controller.CorrectPosition(

                MockIfMatchValidator(true),
                new CorrectBuildingUnitPositionRequestValidator(),
                0,
                request,
                null,
                CancellationToken.None);

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "GebouweendheidPositieValidatie"
                    && e.ErrorMessage == "De verplichte parameter 'positie' ontbreekt."));
        }

        [Fact]
        public void WithPositionHavingInvalidFormat_ThenValidationExceptionIsThrown()
        {
            var request = new CorrectBuildingUnitPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>"
            };

            var streamStoreMock = new Mock<IStreamStore>();
            streamStoreMock.SetStreamFound();

            //Act
            Func<Task> act = async () => await _controller.CorrectPosition(

                MockIfMatchValidator(true),
                new CorrectBuildingUnitPositionRequestValidator(),
                0,
                request,
                null,
                CancellationToken.None);

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "GebouweenheidPositieformaatValidatie"
                    && e.ErrorMessage == "De positie is geen geldige gml-puntgeometrie."));
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitPositionSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            _streamStore.SetStreamFound();

            var correctBuildingUnitPositionRequest = Fixture.Create<CorrectBuildingUnitPositionRequest>();

            var result = (AcceptedResult)await _controller.CorrectPosition(

                MockIfMatchValidator(true),
                MockValidRequestValidator<CorrectBuildingUnitPositionRequest>(),
                0,
                correctBuildingUnitPositionRequest,
                null,
                CancellationToken.None);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public void WithAggregateIdNotFound_ThenValidationErrorIsThrown()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitPositionSqsRequest>(), CancellationToken.None))
                .Throws(new AggregateIdIsNotFoundException());

            _streamStore.SetStreamNotFound();

            var request = Fixture.Create<CorrectBuildingUnitPositionRequest>();

            Func<Task> act = async () =>
            {
                await _controller.CorrectPosition(

                    MockIfMatchValidator(true),
                    MockValidRequestValidator<CorrectBuildingUnitPositionRequest>(),
                    0,
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
        public void WhenAggregateNotFoundException_ThenThrowValidationException()
        {
            MockMediator.Setup<object?>(x => x.Send(It.IsAny<CorrectBuildingUnitPositionSqsRequest>(), CancellationToken.None).Result)
                .Throws(new AggregateNotFoundException("", typeof(Building)));

            var request = Fixture.Create<CorrectBuildingUnitPositionRequest>();

            //Act
            Func<Task> act = async () => await _controller.CorrectPosition(
                MockIfMatchValidator(true),
                MockValidRequestValidator<CorrectBuildingUnitPositionRequest>(),
                0,
                request,
                null,
                CancellationToken.None);

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
        public void WithBuildingUnitNotFound_ThenThrowsApiException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitPositionSqsRequest>(), CancellationToken.None))
                .Throws(new BuildingUnitIsNotFoundException());

            var request = Fixture.Create<CorrectBuildingUnitPositionRequest>();
            Func<Task> act = async () =>
            {
                await _controller.CorrectPosition(
                    MockIfMatchValidator(true),
                    MockValidRequestValidator<CorrectBuildingUnitPositionRequest>(),
                    0,
                    request,
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
    }
}
