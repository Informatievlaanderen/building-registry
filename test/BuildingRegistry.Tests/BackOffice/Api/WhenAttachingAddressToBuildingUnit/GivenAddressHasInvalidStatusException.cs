namespace BuildingRegistry.Tests.BackOffice.Api.WhenAttachingAddressToBuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressHasInvalidStatusException : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenAddressHasInvalidStatusException(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<AttachAddressToBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingRegistry.Building.Exceptions.AddressHasInvalidStatusException());

            //Act
            Func<Task> act = async () =>
            {
                await _controller.AttachAddress(
                    ResponseOptions,
                    MockIfMatchValidator(true),
                    MockValidRequestValidator<AttachAddressToBuildingUnitRequest>(),
                    buildingPersistentLocalId,
                    new AttachAddressToBuildingUnitRequest(),
                    null,
                    CancellationToken.None);
            };

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x => x.Errors.Any(e =>
                    e.ErrorCode == "AdresAfgekeurdGehistoreerd"
                    && e.ErrorMessage == "Enkel een voorgesteld of adres in gebruik kan gekoppeld worden."));
        }
    }
}
