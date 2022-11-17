namespace BuildingRegistry.Tests.BackOffice.Api.WhenChangingBuildingUnitFunction
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using Building;
    using Building.Exceptions;
    using FluentAssertions;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitPlanned : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenBuildingUnitPlanned(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public async Task ThenAcceptedResponseIsExpected()
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeBuildingUnitFunctionRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty, string.Empty));

            //Act
            var result = (AcceptedWithETagResult)await _controller.ChangeFunction(
                ResponseOptions,
                MockIfMatchValidator(true),
                new ChangeBuildingUnitFunctionRequestValidator(),
                buildingUnitPersistentLocalId,
                new ChangeBuildingUnitFunctionRequest { Functie = GebouweenheidFunctie.Verblijfsrecreatie },
                string.Empty,
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<ChangeBuildingUnitFunctionRequest>(), CancellationToken.None), Times.Once);

            result.StatusCode.Should().Be(202);
            result.Location.Should().Be(string.Format(BuildingUnitDetailUrl, buildingUnitPersistentLocalId));
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenReturnsPreconditionFailedResponse()
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            //Act
            var result = (PreconditionFailedResult)await _controller.ChangeFunction(
                ResponseOptions,
                MockIfMatchValidator(false),
                new ChangeBuildingUnitFunctionRequestValidator(),
                buildingUnitPersistentLocalId,
                new ChangeBuildingUnitFunctionRequest { Functie = GebouweenheidFunctie.Verblijfsrecreatie },
                new ETag(ETagType.Strong, "IncorrectIfMatchHeader").ToString(),
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<ChangeBuildingUnitFunctionRequest>(), CancellationToken.None), Times.Never);

            result.Should().NotBeNull();
        }

        [Fact]
        public void WhenBuildingUnitNotFound_ThenThrowsValidationException()
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeBuildingUnitFunctionRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitIsNotFoundException());

            //Act
            Func<Task> act = async () => await _controller.ChangeFunction(
                ResponseOptions,
                MockIfMatchValidator(true),
                new ChangeBuildingUnitFunctionRequestValidator(),
                buildingUnitPersistentLocalId,
                new ChangeBuildingUnitFunctionRequest { Functie = GebouweenheidFunctie.Verblijfsrecreatie },
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
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeBuildingUnitFunctionRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitIsRemovedException(buildingUnitPersistentLocalId));

            //Act
            Func<Task> act = async () => await _controller.ChangeFunction(
                ResponseOptions,
                MockIfMatchValidator(true),
                new ChangeBuildingUnitFunctionRequestValidator(),
                buildingUnitPersistentLocalId,
                new ChangeBuildingUnitFunctionRequest { Functie = GebouweenheidFunctie.Verblijfsrecreatie },
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
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeBuildingUnitFunctionRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitHasInvalidStatusException());

            //Act
            Func<Task> act = async () => await _controller.ChangeFunction(
                ResponseOptions,
                MockIfMatchValidator(true),
                new ChangeBuildingUnitFunctionRequestValidator(),
                buildingUnitPersistentLocalId,
                new ChangeBuildingUnitFunctionRequest { Functie = GebouweenheidFunctie.Verblijfsrecreatie },
                string.Empty,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(error =>
                        error.ErrorCode == "GebouweenheidNietGerealiseerdOfGehistoreerd"
                        && error.ErrorMessage == "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland' of 'gerealiseerd'."));
        }

        [Fact]
        public void WhenBuildingStatusInvalid_ThenThrowsValidationException()
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeBuildingUnitFunctionRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingHasInvalidStatusException());

            //Act
            Func<Task> act = async () => await _controller.ChangeFunction(
                ResponseOptions,
                MockIfMatchValidator(true),
                new ChangeBuildingUnitFunctionRequestValidator(),
                buildingUnitPersistentLocalId,
                new ChangeBuildingUnitFunctionRequest { Functie = GebouweenheidFunctie.Verblijfsrecreatie },
                string.Empty,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(error =>
                        error.ErrorCode == "GebouwStatusNietInGeplandInAanbouwOfGerealiseerd"
                        && error.ErrorMessage == "Deze actie is enkel toegestaan binnen een gepland, inAanbouw of gerealiseerd gebouw."));
        }

        [Fact]
        public void WhenBuildingUnitHasInvalidFunction_ThenThrowsBuildingUnitHasInvalidFunctionException()
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<ChangeBuildingUnitFunctionRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitHasInvalidFunctionException());

            //Act
            Func<Task> act = async () => await _controller.ChangeFunction(
                ResponseOptions,
                MockIfMatchValidator(true),
                new ChangeBuildingUnitFunctionRequestValidator(),
                buildingUnitPersistentLocalId,
                new ChangeBuildingUnitFunctionRequest { Functie = GebouweenheidFunctie.Verblijfsrecreatie },
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
