namespace BuildingRegistry.Tests.BackOffice.Api.BuildingUnit.WhenPlanningBuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Building;
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
        public void
            WithPositionGeometryMethodAppointedByAdministratorAndMissingPosition_ThenValidationExceptionIsThrown(string? position)
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);

            var request = new PlanBuildingUnitRequest
            {
                GebouwId = $"/{buildingPersistentLocalId}",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = position,
                Functie = GebouweenheidFunctie.NietGekend,
                AfwijkingVastgesteld = false
            };

            var streamStoreMock = new Mock<IStreamStore>();
            streamStoreMock.SetStreamFound();

            //Act
            Func<Task> act = async () => await _controller.Plan(

                new PlanBuildingUnitRequestValidator(new BuildingExistsValidator(streamStoreMock.Object)),
                request,
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
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);

            var request = new PlanBuildingUnitRequest
            {
                GebouwId = $"/{buildingPersistentLocalId}",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
                Functie = GebouweenheidFunctie.NietGekend,
                AfwijkingVastgesteld = false
            };

            var streamStoreMock = new Mock<IStreamStore>();
            streamStoreMock.SetStreamFound();

            //Act
            Func<Task> act = async () => await _controller.Plan(

                new PlanBuildingUnitRequestValidator(new BuildingExistsValidator(streamStoreMock.Object)),
                request,
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
                .Setup(x => x.Send(It.IsAny<PlanBuildingUnitSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            _streamStore.SetStreamFound();

            var planBuildingUnitRequest = Fixture.Create<PlanBuildingUnitRequest>();
            planBuildingUnitRequest.GebouwId = "https://bla/1";

            var result = (AcceptedResult)await _controller.Plan(

                MockValidRequestValidator<PlanBuildingUnitRequest>(),
                planBuildingUnitRequest);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public async Task WithNonExistingBuildingPersistentLocalId_ThenValidationErrorIsThrown()
        {
            var planBuildingUnitRequest = Fixture.Create<PlanBuildingUnitRequest>();
            planBuildingUnitRequest.GebouwId = "https://bla/1";

            //Arrange
            _streamStore.SetStreamNotFound();

            //Act
            var act = async () => await _controller.Plan(

                new PlanBuildingUnitRequestValidator(new BuildingExistsValidator(_streamStore.Object)),
                planBuildingUnitRequest,
                CancellationToken.None);

            //Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "GebouweenheidGebouwIdNietGekendValidatie"
                    && e.ErrorMessage == $"De gebouwId '{planBuildingUnitRequest.GebouwId}' is niet gekend in het gebouwenregister."));
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            MockMediator.Setup<object?>(x => x.Send(It.IsAny<PlanBuildingUnitSqsRequest>(), CancellationToken.None).Result)
                .Throws(new AggregateNotFoundException("", typeof(Building)));

            var request = new PlanBuildingUnitRequest
            {
                GebouwId = $"https://data.vlaanderen.be/id/gebouw/{new BuildingPersistentLocalId(123)}",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
                Functie = GebouweenheidFunctie.NietGekend,
                AfwijkingVastgesteld = false
            };

            //Act
            Func<Task> act = async () => await _controller.Plan(

                MockValidRequestValidator<PlanBuildingUnitRequest>(),
                request,
                CancellationToken.None);

            //Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message.Contains("Onbestaand gebouw.")
                    && x.StatusCode == StatusCodes.Status400BadRequest);
        }
    }
}
