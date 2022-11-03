namespace BuildingRegistry.Tests.BackOffice.Api.WhenNotRealizingBuildingUnit
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
    using Building;
    using Building.Events;
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
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
                .Setup(x => x.Send(It.IsAny<NotRealizeBuildingUnitRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty, string.Empty));

            var request = new NotRealizeBuildingUnitRequest
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            var result = (AcceptedWithETagResult)await _controller.NotRealize(
                ResponseOptions,
                MockIfMatchValidator(true),
                new NotRealizeBuildingUnitRequestValidator(),
                request,
                string.Empty,
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<NotRealizeBuildingUnitRequest>(), CancellationToken.None), Times.Once);

            result.StatusCode.Should().Be(202);
            result.Location.Should().Be(string.Format(BuildingUnitDetailUrl, buildingUnitPersistentLocalId));
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            Fixture.Register(() => buildingUnitPersistentLocalId);

            var building = new BuildingFactory(NoSnapshotStrategy.Instance, Mock.Of<IAddCommonBuildingUnit>()).Create();
            var buildingUnitPlanned = Fixture.Create<BuildingUnitWasPlannedV2>();
            var ifMatchHeader = "IncorrectIfMatchHeader";

            building.Initialize(new[]
            {
                buildingUnitPlanned
            });

            MockMediator
                .Setup(x => x.Send(It.IsAny<NotRealizeBuildingUnitRequest>(), CancellationToken.None).Result)
                .Returns(new ETagResponse(string.Empty, string.Empty));

            var request = new NotRealizeBuildingUnitRequest
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act

            var result = (PreconditionFailedResult)await _controller.NotRealize(
                ResponseOptions,
                MockIfMatchValidator(false),
                new NotRealizeBuildingUnitRequestValidator(),
                request,
                new ETag(ETagType.Strong, ifMatchHeader).ToString(),
                CancellationToken.None);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<NotRealizeBuildingUnitRequest>(), CancellationToken.None), Times.Never);

            result.Should().NotBeNull();
        }

        [Fact]
        public void WhenBuildingUnitNotFound_ThenValidationException()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<NotRealizeBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitIsNotFoundException());

            var request = new NotRealizeBuildingUnitRequest
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.NotRealize(
                ResponseOptions,
                MockIfMatchValidator(true),
                new NotRealizeBuildingUnitRequestValidator(),
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
        public void WhenBuildingUnitIsRemoved_ThenValidationException()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<NotRealizeBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitIsRemovedException(buildingUnitPersistentLocalId));

            var request = new NotRealizeBuildingUnitRequest
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.NotRealize(
                ResponseOptions,
                MockIfMatchValidator(true),
                new NotRealizeBuildingUnitRequestValidator(),
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
        public void WhenBuildingUnitStatusInvalid_ThenValidationException()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<NotRealizeBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitHasInvalidStatusException());

            var request = new NotRealizeBuildingUnitRequest
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.NotRealize(
                ResponseOptions,
                MockIfMatchValidator(true),
                new NotRealizeBuildingUnitRequestValidator(),
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
                        => error.ErrorCode == "GebouweenheidGehistoreerdOfGerealiseerd"
                                          && error.ErrorMessage == "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland'."));
        }

        [Fact]
        public void WhenBuildingUnitHasInvalidFunction_ThenThrowsBuildingUnitHasInvalidFunctionException()
        {
            var buildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            MockMediator
                .Setup(x => x.Send(It.IsAny<NotRealizeBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitHasInvalidFunctionException());

            var request = new NotRealizeBuildingUnitRequest()
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.NotRealize(
                ResponseOptions,
                MockIfMatchValidator(true),
                new NotRealizeBuildingUnitRequestValidator(),
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
