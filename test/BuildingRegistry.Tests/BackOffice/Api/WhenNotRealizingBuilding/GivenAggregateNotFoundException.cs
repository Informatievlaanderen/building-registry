namespace BuildingRegistry.Tests.BackOffice.Api.WhenNotRealizingBuilding
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Building;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAggregateNotFoundException : BackOfficeApiTest
    {
        private readonly BuildingController _controller;

        public GivenAggregateNotFoundException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public void ThenThrowsApiException()
        {
            var buildingPersistentLocalId = new BuildingPersistentLocalId(123);

            var request = new NotRealizeBuildingRequest
            {
                PersistentLocalId = buildingPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<NotRealizeBuildingRequest>(), CancellationToken.None).Result)
                .Throws(new AggregateNotFoundException(buildingPersistentLocalId, typeof(Building)));

            //Act
            Func<Task> act = async () => await _controller.NotRealize(ResponseOptions,
                MockValidRequestValidator<NotRealizeBuildingRequest>(),
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
                    x.StatusCode == StatusCodes.Status404NotFound
                    && x.Message == "Onbestaand gebouw.");
        }
    }
}
