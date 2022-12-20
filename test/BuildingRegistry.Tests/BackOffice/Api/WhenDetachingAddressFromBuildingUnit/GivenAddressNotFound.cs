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
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressNotFound : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenAddressNotFound(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<DetachAddressFromBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new AddressNotFoundException());

            //Act
            Func<Task> act = async () =>
            {
                await _controller.DetachAddress(
                    ResponseOptions,
                    MockIfMatchValidator(true),
                    new DetachAddressFromBuildingUnitRequestValidator(Mock.Of<IAddresses>()),
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    new DetachAddressFromBuildingUnitRequest(),
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

        [Fact]
        public void GivenAddressNotFoundException_ThenThrowValidationException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<DetachAddressFromBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new AddressNotFoundException());

            //Act
            Func<Task> act = async () =>
            {
                await _controller.DetachAddress(
                    ResponseOptions,
                    MockIfMatchValidator(true),
                    MockValidRequestValidator<DetachAddressFromBuildingUnitRequest>(),
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
                    new DetachAddressFromBuildingUnitRequest(),
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
