namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingPlaceBuildingUnderConstruction
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Building;
    using Building;
    using Building.Exceptions;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingIsRemovedException : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenBuildingIsRemovedException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public void ThenThrowApiException()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);

            var request = new CorrectPlaceBuildingUnderConstructionRequest
            {
                PersistentLocalId = buildingPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectPlaceBuildingUnderConstructionRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingIsRemovedException(buildingPersistentLocalId));

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
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.StatusCode == StatusCodes.Status410Gone
                    && x.Message == "Verwijderd gebouw.");
        }
    }
}