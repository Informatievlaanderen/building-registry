namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingPlaceBuildingUnderConstruction
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Building;
    using Building;
    using Building.Exceptions;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingCannotCorrectPlaceUnderConstructionException : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenBuildingCannotCorrectPlaceUnderConstructionException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public void ThenValidationException()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);

            var request = new CorrectPlaceBuildingUnderConstructionRequest
            {
                PersistentLocalId = buildingPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectPlaceBuildingUnderConstructionRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingHasInvalidStatusException());

            //Act
            Func<Task> act = async () => await _controller.CorrectPlaceUnderConstruction(ResponseOptions,
                MockValidRequestValidator<CorrectPlaceBuildingUnderConstructionRequest>(),
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
                        failure => failure.ErrorCode == "GebouwGeplandOfGerealiseerdOfGehistoreerdOfNietgerealiseerd"
                                    && failure.ErrorMessage == "Deze actie is enkel toegestaan op gebouwen met status 'inAanbouw'."));
        }
    }
}
