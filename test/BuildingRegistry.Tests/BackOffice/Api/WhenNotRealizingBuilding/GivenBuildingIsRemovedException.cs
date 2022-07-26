namespace BuildingRegistry.Tests.BackOffice.Api.WhenNotRealizingBuilding
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using Building;
    using Building.Exceptions;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
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

            var request = new NotRealizeBuildingRequest()
            {
                PersistentLocalId = buildingPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<NotRealizeBuildingRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingIsRemovedException(buildingPersistentLocalId));

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
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                    x.StatusCode == StatusCodes.Status410Gone
                    && x.Message == "Verwijderd gebouw.");
        }
    }
}
