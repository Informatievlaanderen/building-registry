namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingUnitRetirement
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
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
            _controller = CreateBuildingControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenThrowApiException()
        {
            var buildingPersistentLocalId = new BuildingUnitPersistentLocalId(456);

            var request = new CorrectBuildingUnitRetirementRequest()
            {
                BuildingUnitPersistentLocalId = buildingPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitRetirementRequest>(), CancellationToken.None).Result)
                .Throws(new AggregateNotFoundException(buildingPersistentLocalId, typeof(Building)));

            //Act
            Func<Task> act = async () => await _controller.CorrectRetirement(
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
                        e.ErrorCode == "GebouweenheidGebouwIdNietGekendValidatie"
                        && e.ErrorMessage == "Onbestaand gebouw."));
        }
    }
}
