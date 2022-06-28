namespace BuildingRegistry.Tests.BackOffice.Api.WhenRealizingBuilding
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Building;
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingIsRemovedException : BuildingRegistryBackOfficeTest
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

            var request = new RealizeBuildingRequest()
            {
                PersistentLocalId = buildingPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<RealizeBuildingRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingIsRemovedException(buildingPersistentLocalId));

            //Act
            Func<Task> act = async () => await _controller.Realize(
                Container.Resolve<IBuildings>(),
                ResponseOptions,
                new RealizeBuildingRequestValidator(),
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
