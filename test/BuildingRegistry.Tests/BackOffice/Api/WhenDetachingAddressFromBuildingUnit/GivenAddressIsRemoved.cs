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
    using Building.Datastructures;
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Validators;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressIsRemoved : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenAddressIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            var address = new AddressData(Fixture.Create<AddressPersistentLocalId>(), AddressStatus.Current, IsRemoved: true);
            var addresses = new Mock<IAddresses>();
            addresses.Setup(x => x.GetOptional(address.AddressPersistentLocalId)).Returns(address);

            //Act
            Func<Task> act = async () =>
            {
                await _controller.DetachAddress(
                    ResponseOptions,
                    MockIfMatchValidator(true),
                    new DetachAddressFromBuildingUnitRequestValidator(addresses.Object),
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
        public void GivenAddressIsRemovedException_ThenThrowValidationException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<DetachAddressFromBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new AddressIsRemovedException());

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
