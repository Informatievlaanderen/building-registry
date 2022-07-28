namespace BuildingRegistry.Tests.BackOffice.Api.WhenNotRealizingBuilding
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using Building;
    using Building.Exceptions;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingStatusPreventsNotRealizeBuildingException : BuildingRegistryBackOfficeTest
    {
        private readonly BuildingController _controller;

        public GivenBuildingStatusPreventsNotRealizeBuildingException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public void ThenValidationException()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);

            var request = new NotRealizeBuildingRequest()
            {
                PersistentLocalId = buildingPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<NotRealizeBuildingRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingStatusPreventsNotRealizeBuildingException());

            //Act
            Func<Task> act = async () => await _controller.NotRealize(
                Container.Resolve<IBuildings>(),
                ResponseOptions,
                new NotRealizeBuildingRequestValidator(),
                MockIfMatchValidator(true),
                request,
                null,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(
                        failure => failure.ErrorCode == "GebouwGehistoreerdOfGerealiseerd"
                                    && failure.ErrorMessage == "Deze actie is enkel toegestaan op gebouwen met status 'gepland' of 'inAanbouw'."));
        }
    }
}
