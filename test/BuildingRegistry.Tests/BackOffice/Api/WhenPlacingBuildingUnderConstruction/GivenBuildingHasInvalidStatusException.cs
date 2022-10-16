namespace BuildingRegistry.Tests.BackOffice.Api.WhenPlacingBuildingUnderConstruction
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Building;
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Building;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingCannotBePlaceUnderConstructionException : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenBuildingCannotBePlaceUnderConstructionException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public void ThenValidationException()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);

            var request = new PlaceBuildingUnderConstructionRequest
            {
                PersistentLocalId = buildingPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<PlaceBuildingUnderConstructionRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingHasInvalidStatusException());

            //Act
            Func<Task> act = async () => await _controller.UnderConstruction(ResponseOptions,
                MockValidRequestValidator<PlaceBuildingUnderConstructionRequest>(),
                null,
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
                        failure => failure.ErrorCode == "GebouwGehistoreerdGerealiseerdOfNietGerealiseerd"
                                    && failure.ErrorMessage == "Deze actie is enkel toegestaan op gebouwen met status 'gepland'."));
        }
    }
}
