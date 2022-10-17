namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingNotRealization
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Building;
    using Building;
    using Building.Exceptions;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingHasInvalidStatusException : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenBuildingHasInvalidStatusException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public void ThenValidationException()
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var request = new CorrectBuildingNotRealizationRequest
            {
                PersistentLocalId = buildingPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingNotRealizationRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingHasInvalidStatusException());

            //Act
            Func<Task> act = async () => await _controller.CorrectNotRealization(
                ResponseOptions,
                MockValidRequestValidator<CorrectBuildingNotRealizationRequest>(),
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
                        failure => failure.ErrorCode == "GebouwInaanbouwGerealiseerdOfGehistoreerd"
                                    && failure.ErrorMessage == "Deze actie is enkel toegestaan op geschetste gebouwen met status 'nietGerealiseerd'."));
        }
    }
}
