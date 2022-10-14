namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingRealization
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

    public class GivenBuildingHasInvalidBuildingGeometryMethod : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenBuildingHasInvalidBuildingGeometryMethod(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public void ThenValidationException()
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            var request = new CorrectBuildingRealizationRequest
            {
                PersistentLocalId = buildingPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingRealizationRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingHasInvalidBuildingGeometryMethodException());

            //Act
            Func<Task> act = async () => await _controller.CorrectRealization(
                ResponseOptions,
                MockValidRequestValidator<CorrectBuildingRealizationRequest>(),
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
                        failure => failure.ErrorCode == "GebouwGeometrieIngemeten"
                                    && failure.ErrorMessage == "Deze actie is enkel toegestaan op gebouwen met geometriemethode 'ingeschetst'."));
        }
    }
}
