namespace BuildingRegistry.Tests.BackOffice.Api.WhenRetiringBuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building;
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingHasInvalidStatusException : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenBuildingHasInvalidStatusException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenThrowApiException()
        {
            var buildingPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            var request = new RetireBuildingUnitRequest
            {
                BuildingUnitPersistentLocalId = buildingPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<RetireBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingHasInvalidStatusException());

            //Act
            Func<Task> act = async () => await _controller.Retire(
                ResponseOptions,
                MockIfMatchValidator(true),
                request,
                string.Empty,
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                    x.Errors.Any(e =>
                        e.ErrorCode == "GebouwStatusNietInGeplandInAanbouwOfGerealiseerd"
                        && e.ErrorMessage == "Deze actie is enkel toegestaan binnen een gepland, inAanbouw of gerealiseerd gebouw."));
        }
    }
}
