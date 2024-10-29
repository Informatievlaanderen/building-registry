namespace BuildingRegistry.Tests.BackOffice.Sqs.Building
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AllStream;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Building;
    using BuildingRegistry.Building;
    using Fixtures;
    using FluentAssertions;
    using Moq;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenCreateBuildingOsloSnapshotsBackOfficeRequest : BuildingRegistryTest
    {
        public GivenCreateBuildingOsloSnapshotsBackOfficeRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
            Fixture.Customize(new WithFixedBuildingPersistentLocalId());
        }

        [Fact]
        public async Task ThenTicketWithLocationIsCreated()
        {
            // Arrange
            var ticketId = Fixture.Create<Guid>();
            var ticketingMock = new Mock<ITicketing>();
            ticketingMock
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);

            var ticketingUrl = new TicketingUrl(Fixture.Create<Uri>().ToString());

            var sqsQueue = new Mock<ISqsQueue>();

            var sut = new CreateBuildingOsloSnapshotsSqsHandler(
                sqsQueue.Object,
                ticketingMock.Object,
                ticketingUrl);

            var sqsRequest = new CreateBuildingOsloSnapshotsSqsRequest
            {
                Request = new CreateBuildingOsloSnapshotsRequest
                {
                    BuildingPersistentLocalIds = Fixture.CreateMany<BuildingPersistentLocalId>().Select(x => (int)x).ToList()
                }
            };

            // Act
            var result = await sut.Handle(sqsRequest, CancellationToken.None);

            // Assert
            sqsRequest.TicketId.Should().Be(ticketId);
            sqsQueue.Verify(x => x.Copy(
                sqsRequest,
                It.Is<SqsQueueOptions>(y => y.MessageGroupId == AllStreamId.Instance.ToString()),
                CancellationToken.None));
            result.Location.Should().Be(ticketingUrl.For(ticketId));
        }
    }
}
