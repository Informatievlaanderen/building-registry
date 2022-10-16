namespace BuildingRegistry.Tests.BackOffice.Api.WhenPlanningBuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Building;
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitCannotBePlannedException : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenBuildingUnitCannotBePlannedException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            MockMediator.Setup<object?>(x => x.Send(It.IsAny<PlanBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingHasInvalidStatusException());

            var request = new PlanBuildingUnitRequest
            {
                GebouwId = $"https://data.vlaanderen.be/id/gebouw/{new BuildingPersistentLocalId(123)}",
                PositieGeometrieMethode = PositieGeometrieMethode.AangeduidDoorBeheerder,
                Positie = "<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\"><gml:pos>103671.37 192046.71</gml:pos></gml:Point>",
                Functie = GebouweenheidFunctie.NietGekend,
                AfwijkingVastgesteld = false
            };

            //Act
            Func<Task> act = async () => await _controller.Plan(
                ResponseOptions,
                MockValidRequestValidator<PlanBuildingUnitRequest>(),
                request,
                CancellationToken.None);

            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "GebouweenheidGebouwIdNietGerealiseerdofGehistoreerd"
                    && e.ErrorMessage == "De gebouwId is niet gerealiseerd of gehistoreerd."));
        }
    }
}
