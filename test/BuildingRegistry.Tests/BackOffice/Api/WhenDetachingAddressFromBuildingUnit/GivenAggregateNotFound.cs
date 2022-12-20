namespace BuildingRegistry.Tests.BackOffice.Api.WhenDetachingAddressFromBuildingUnit
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using Building;
    using FluentAssertions;
    using FluentValidation;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAggregateNotFound : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenAggregateNotFound(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateBuildingControllerWithUser<BuildingUnitController>();
        }

        [Fact]
        public void ThenThrowValidationException()
        {
            MockMediator
                .Setup(x => x.Send(It.IsAny<AttachAddressToBuildingUnitRequest>(), CancellationToken.None).Result)
                .Throws(new AggregateNotFoundException(Fixture.Create<BuildingPersistentLocalId>(), typeof(Building)));

            //Act
            Func<Task> act = async () =>
            {
                await _controller.AttachAddress(
                    ResponseOptions,
                    MockIfMatchValidator(true),
                    MockValidRequestValidator<AttachAddressToBuildingUnitRequest>(),
                    Fixture.Create<BuildingUnitPersistentLocalId>(),
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
                    e.ErrorCode == "GebouweenheidGebouwIdNietGekendValidatie"
                    && e.ErrorMessage == "Onbestaand gebouw."));
        }
    }
}
