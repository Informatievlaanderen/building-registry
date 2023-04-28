namespace BuildingRegistry.Tests.BackOffice.Api.Building.WhenMeasuringBuilding
{
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using BuildingRegistry.Building;
    using Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using SqlStreamStore;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingDoesNotExist : BackOfficeApiTest
    {
        private readonly BuildingController _controller;
        private readonly Mock<IStreamStore> _streamStore;

        public GivenBuildingDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _controller = CreateBuildingControllerWithUser<BuildingController>();
            _streamStore = new Mock<IStreamStore>();
        }

        [Fact]
        public void ThenThrowsApiException()
        {
            _streamStore.SetStreamNotFound();

            var act = async () => await _controller.Measure(
                MockValidRequestValidator<MeasureBuildingRequest>(),
                new BuildingExistsValidator(_streamStore.Object),
                Fixture.Create<BuildingPersistentLocalId>(),
                Fixture.Create<MeasureBuildingRequest>());

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
