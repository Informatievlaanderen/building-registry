namespace BuildingRegistry.Tests.BackOffice.Api.WhenPlanningBuilding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using BackOffice;
    using Building;
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Responses;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators;
    using BuildingRegistry.Api.BackOffice.Building;
    using FluentAssertions;
    using FluentValidation;
    using FluentValidation.Results;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenNoBuildingExists : BuildingRegistryBackOfficeTest
    {
        private readonly BuildingController _controller;

        public GivenNoBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingController>();
        }

        [Fact]
        public async Task ThenBuildingIsPlanned()
        {
            const int expectedLocation = 5;
            const string expectedHash = "123456";

            //Arrange
            var mockPersistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            mockPersistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(expectedLocation);

            MockMediator
                .Setup(x => x.Send(It.IsAny<PlanBuildingRequest>(), CancellationToken.None).Result)
                .Returns(new PlanBuildingResponse(expectedLocation, expectedHash));

            var body = new PlanBuildingRequest
            {
                GeometriePolygoon = "<gml:Polygon srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:exterior><gml:LinearRing><gml:posList>140284.15277253836 186724.74131567031 140291.06016454101 186726.38355567306 140288.22675654292 186738.25798767805 140281.19098053873 186736.57913967967 140284.15277253836 186724.74131567031</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>"
            };

            //Act
            var result = (AcceptedWithETagResult)await _controller.Plan(
                ResponseOptions,
                new PlanBuildingRequestValidator(),
                body);

            //Assert
            MockMediator.Verify(x => x.Send(It.IsAny<PlanBuildingRequest>(), CancellationToken.None), Times.Once);

            result.StatusCode.Should().Be(202);
            result.Location.Should().Be(string.Format(BuildingDetailUrl, expectedLocation));
            result.ETag.Should().Be(expectedHash);
        }

        [Fact]
        public void WithInvalidGeometry_ThrowsValidationException()
        {
            const int expectedLocation = 5;

            //Arrange
            var mockPersistentLocalIdGenerator = new Mock<IPersistentLocalIdGenerator>();
            mockPersistentLocalIdGenerator
                .Setup(x => x.GenerateNextPersistentLocalId())
                .Returns(expectedLocation);

            MockMediator
                .Setup(x => x.Send(It.IsAny<PlanBuildingRequest>(), CancellationToken.None).Result)
                .Throws(new InvalidPolygonException());

            var body = new PlanBuildingRequest
            {
                GeometriePolygoon = ""
            };

            //Act
            Func<Task> act = async () => await _controller.Plan(
                ResponseOptions,
                new PlanBuildingRequestValidator(),
                body,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(
                        failure => failure.ErrorCode == "GebouwPolygoonValidatie"
                                   && failure.ErrorMessage == "Ongeldig formaat geometriePolygoon."));

            MockMediator.Verify(x => x.Send(It.IsAny<PlanBuildingRequest>(), CancellationToken.None), Times.Never);
        }
    }
}
