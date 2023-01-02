namespace BuildingRegistry.Tests.BackOffice.Api.WhenRemovingBuilding
{
    using System.Threading;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using SqlStreamStore;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingNotFound : BackOfficeApiTest
    {
        private readonly BuildingController _controller;
        private readonly Mock<IStreamStore> _streamStore;

        public GivenBuildingNotFound(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _streamStore = new Mock<IStreamStore>();
            _controller = CreateBuildingControllerWithUser<BuildingController>(useSqs: true);
        }

        [Fact]
        public void ThenValidationErrorIsThrown()
        {
            //Arrange
            _streamStore.SetStreamNotFound();

            //Act
            var act = async () => await _controller.Remove(
                new BuildingExistsValidator(_streamStore.Object),
                MockIfMatchValidator(true),
                Fixture.Create<RemoveBuildingRequest>(),
                null,
                CancellationToken.None);

            //Assert
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
