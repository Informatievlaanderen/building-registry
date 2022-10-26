namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingUnitNotRealization
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using Building;
    using Building.Events;
    using Building.Exceptions;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitNotRealized : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenBuildingUnitNotRealized(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public async Task ThenAcceptedResponseIsExpected()
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitNotRealizationRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty, string.Empty));

            //Act
            var result = (AcceptedWithETagResult)await _controller.CorrectNotRealization(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitNotRealizationRequestValidator(),
                new CorrectBuildingUnitNotRealizationRequest { BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId },
                string.Empty,
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<CorrectBuildingUnitNotRealizationRequest>(), CancellationToken.None), Times.Once);

            result.StatusCode.Should().Be(202);
            result.Location.Should().Be(string.Format(BuildingUnitDetailUrl, buildingUnitPersistentLocalId));
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenReturnsPreconditionFailedResponse()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitNotRealizationRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty, string.Empty));

            //Act
            var result = (PreconditionFailedResult)await _controller.CorrectNotRealization(
                ResponseOptions,
                MockIfMatchValidator(false),
                new CorrectBuildingUnitNotRealizationRequestValidator(),
                Fixture.Create<CorrectBuildingUnitNotRealizationRequest>(),
                new ETag(ETagType.Strong, "IncorrectIfMatchHeader").ToString(),
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<CorrectBuildingUnitNotRealizationRequest>(), CancellationToken.None), Times.Never);

            result.Should().NotBeNull();
        }

        [Fact]
        public void WhenBuildingUnitNotFound_ThenThrowsValidationException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitNotRealizationRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitIsNotFoundException());

            //Act
            Func<Task> act = async () => await _controller.CorrectNotRealization(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitNotRealizationRequestValidator(),
                Fixture.Create<CorrectBuildingUnitNotRealizationRequest>(),
                string.Empty,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message == "Onbestaande gebouweenheid."
                    && x.StatusCode == StatusCodes.Status404NotFound);
        }

        [Fact]
        public void WhenBuildingUnitIsRemoved_ThenThrowsValidationException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitNotRealizationRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitIsRemovedException());

            //Act
            Func<Task> act = async () => await _controller.CorrectNotRealization(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitNotRealizationRequestValidator(),
                Fixture.Create<CorrectBuildingUnitNotRealizationRequest>(),
                string.Empty,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.Message == "Verwijderde gebouweenheid."
                    && x.StatusCode == StatusCodes.Status410Gone);
        }

        [Fact]
        public void WhenBuildingUnitStatusInvalid_ThenThrowsValidationException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitNotRealizationRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitHasInvalidStatusException());

            //Act
            Func<Task> act = async () => await _controller.CorrectNotRealization(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitNotRealizationRequestValidator(),
                Fixture.Create<CorrectBuildingUnitNotRealizationRequest>(),
                string.Empty,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(error
                        => error.ErrorCode == "GebouweenheidGerealiseerdOfGehistoreerd"
                           && error.ErrorMessage == "Deze actie is enkel toegestaan op gebouweenheden met status 'nietGerealiseerd'."));
        }

        [Fact]
        public void WhenBuildingUnitHasInvalidFunction_ThenThrowsBuildingUnitHasInvalidFunctionException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitNotRealizationRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitHasInvalidFunctionException());

            //Act
            Func<Task> act = async () => await _controller.CorrectNotRealization(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitNotRealizationRequestValidator(),
                Fixture.Create<CorrectBuildingUnitNotRealizationRequest>(),
                string.Empty,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(error
                        => error.ErrorCode == "GebouweenheidGemeenschappelijkDeel"
                           && error.ErrorMessage == "Deze actie is niet toegestaan op gebouweenheden met functie gemeenschappelijkDeel."));
        }
    }
}
