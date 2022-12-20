namespace BuildingRegistry.Tests.BackOffice.Api.WhenDetachingAddressFromBuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenInvalidAddressId : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenInvalidAddressId(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            //Act
            Func<Task> act = async () =>
            {
                await _controller.DetachAddress(
                    ResponseOptions,
                    MockIfMatchValidator(true),
                    new DetachAddressFromBuildingUnitRequestValidator(Mock.Of<IAddresses>()),
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    new DetachAddressFromBuildingUnitRequest {AdresId = "https://invalid.vlaanderen.be/notAnId/notAnAddress/xyz" },
                    null,
                    CancellationToken.None);
            };

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "GebouweenheidAdresOngeldig"
                    && e.ErrorMessage == "Ongeldig adresId."));
        }
    }
}
