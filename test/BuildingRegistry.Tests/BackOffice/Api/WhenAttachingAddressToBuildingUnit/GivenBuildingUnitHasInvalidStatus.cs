namespace BuildingRegistry.Tests.BackOffice.Api.WhenAttachingAddressToBuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Building;
    using Building.Exceptions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingUnitHasInvalidStatus : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenBuildingUnitHasInvalidStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            var buildingPersistentLocalId = Fixture.Create<BuildingPersistentLocalId>();

            MockMediator
                .Setup(x => x.Send(It.IsAny<AttachAddressToBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new BuildingUnitHasInvalidStatusException());

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
                    e.ErrorCode == "GebouweenheidNietGerealiseerdOfGehistoreerd"
                    && e.ErrorMessage == "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland' of 'gerealiseerd'."));
        }
    }
}
