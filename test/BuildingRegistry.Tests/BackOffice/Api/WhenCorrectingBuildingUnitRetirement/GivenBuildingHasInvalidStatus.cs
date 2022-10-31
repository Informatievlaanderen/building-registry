namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingUnitRetirement
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Building;
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class GivenBuildingHasInvalidStatusException : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenBuildingHasInvalidStatusException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenValidationException()
        {
            var buildingUnitPersistentLocalId = Fixture.Create<BuildingUnitPersistentLocalId>();

            var request = new CorrectBuildingUnitRetirementRequest
            {
                BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId
            };

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitRetirementRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingHasInvalidStatusException());

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
                    x.Errors.Any(
                        failure => failure.ErrorCode == "GebouwStatusNietInGerealiseerd"
                                   && failure.ErrorMessage == "Deze actie is enkel toegestaan binnen een gerealiseerd gebouw."));
        }
    }
}
