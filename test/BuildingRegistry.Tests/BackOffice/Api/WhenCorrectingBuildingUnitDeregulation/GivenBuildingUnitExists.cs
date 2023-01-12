namespace BuildingRegistry.Tests.BackOffice.Api.WhenCorrectingBuildingUnitDeregulation
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit;
    using BuildingRegistry.Tests.Fixtures;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenBuildingExists : BackOfficeApiTest
    {
        private readonly BuildingUnitController _controller;

        public GivenBuildingExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());

            _controller = CreateBuildingUnitControllerWithUser<BuildingUnitController>(useSqs: true);
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<CorrectBuildingUnitDeregulationSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var result = (AcceptedResult)await _controller.CorrectDeregulation(
                ResponseOptions,
                MockIfMatchValidator(true),
                Fixture.Create<CorrectBuildingUnitDeregulationRequest>(),
                ifMatchHeaderValue: null);

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }

        [Fact]
        public async Task WithInvalidIfMatchHeader_ThenPreconditionFailedResponse()
        {
            //Act
            var result = await _controller.CorrectDeregulation(
                ResponseOptions,
                MockIfMatchValidator(false),
                Fixture.Create<CorrectBuildingUnitDeregulationRequest>(),
                "IncorrectIfMatchHeader");

            //Assert
            result.Should().BeOfType<PreconditionFailedResult>();
        }
    }
}
