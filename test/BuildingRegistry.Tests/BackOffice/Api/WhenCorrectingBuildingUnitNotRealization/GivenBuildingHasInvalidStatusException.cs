namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingUnitNotRealization
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Exceptions;
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
            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            MockMediator.Setup<object?>(x => x.Send(It.IsAny<CorrectBuildingUnitNotRealizationRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingHasInvalidStatusException());

            var request = new CorrectBuildingUnitNotRealizationRequest()
            {
                BuildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>()
            };

            //Act
            Func<Task> act = async () => await _controller.CorrectNotRealization(
                ResponseOptions,
                MockIfMatchValidator(true),
                MockValidRequestValidator<CorrectBuildingUnitNotRealizationRequest>(),
                request,
                null,
                CancellationToken.None);

            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "GebouwStatusNietInGeplandInAanbouwOfGerealiseerd"
                        && e.ErrorMessage == "Deze actie is enkel toegestaan binnen een gepland, inAanbouw of gerealiseerd gebouw."));
        }
    }
}
