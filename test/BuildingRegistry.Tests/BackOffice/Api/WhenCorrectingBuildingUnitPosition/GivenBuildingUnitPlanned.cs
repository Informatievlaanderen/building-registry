namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingUnitPosition
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using BuildingRegistry.Building.Exceptions;
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
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitPositionRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty, string.Empty));

            var request = new CorrectBuildingUnitPositionRequest()
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            var result = (AcceptedWithETagResult)await _controller.CorrectPosition(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitPositionRequestValidator(),
                request,
                string.Empty,
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<CorrectBuildingUnitPositionRequest>(), CancellationToken.None), Times.Once);

            result.StatusCode.Should().Be(202);
            result.Location.Should().Be(string.Format(BuildingUnitDetailUrl, buildingUnitPersistentLocalId));
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenReturnsPreconditionFailedResponse()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            Fixture.Register(() => buildingUnitPersistentLocalId);

            var building = new BuildingFactory(NoSnapshotStrategy.Instance).Create();
            var buildingUnitPlanned = Fixture.Create<BuildingUnitWasPlannedV2>();
            var ifMatchHeader = "IncorrectIfMatchHeader";

            building.Initialize(new[]
            {
                buildingUnitPlanned
            });

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitPositionRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty, string.Empty));

            var request = new CorrectBuildingUnitPositionRequest()
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act

            var result = (PreconditionFailedResult)await _controller.CorrectPosition(
                ResponseOptions,
                MockIfMatchValidator(false),
                new CorrectBuildingUnitPositionRequestValidator(),
                request,
                new ETag(ETagType.Strong, ifMatchHeader).ToString(),
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<CorrectBuildingUnitPositionRequest>(), CancellationToken.None), Times.Never);

            result.Should().NotBeNull();
        }

        [Fact]
        public void WhenBuildingUnitNotFound_ThenThrowsValidationException()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitPositionRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitIsNotFoundException());

            var request = new CorrectBuildingUnitPositionRequest()
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.CorrectPosition(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitPositionRequestValidator(),
                request,
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
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitPositionRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitIsRemovedException(buildingUnitPersistentLocalId));

            var request = new CorrectBuildingUnitPositionRequest()
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.CorrectPosition(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitPositionRequestValidator(),
                request,
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
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitPositionRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitHasInvalidStatusException());

            var request = new CorrectBuildingUnitPositionRequest()
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.CorrectPosition(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitPositionRequestValidator(),
                request,
                string.Empty,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(error
                        => error.ErrorCode == "GebouweenheidNietGerealiseerdOfGehistoreerd"
                           && error.ErrorMessage == "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland' of 'gerealiseerd'."));
        }

        [Fact]
        public void WhenBuildingUnitHasInvalidFunction_ThenThrowsBuildingUnitHasInvalidFunctionException()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitPositionRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitHasInvalidFunctionException());

            var request = new CorrectBuildingUnitPositionRequest
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.CorrectPosition(
                ResponseOptions,
                MockIfMatchValidator(true),
                new CorrectBuildingUnitPositionRequestValidator(),
                request,
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
