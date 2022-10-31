namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingUnitPosition
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using Building;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAggregateNotFoundException : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenAggregateNotFoundException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            MockMediator.Setup<object?>(x => x.Send(It.IsAny<CorrectBuildingUnitPositionRequest>(), CancellationToken.None).Result)
                .Throws(new AggregateNotFoundException("", typeof(Building)));

            var request = new CorrectBuildingUnitPositionRequest
            {
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>"
            };

            //Act
            Func<Task> act = async () => await _controller.CorrectPosition(
                ResponseOptions,
                MockIfMatchValidator(true),
                MockValidRequestValidator<CorrectBuildingUnitPositionRequest>(),
                0,
                request,
                null,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "GebouweenheidGebouwIdNietGekendValidatie"
                    && e.ErrorMessage == "Onbestaand gebouw."));
        }
    }
}
