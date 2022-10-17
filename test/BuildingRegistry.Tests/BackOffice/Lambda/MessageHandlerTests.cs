namespace BuildingRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit;
    using FluentAssertions;
    using MediatR;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public sealed class MessageHandlerTests : BuildingRegistryTest
    {
        public MessageHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        { }

        [Fact]
        public async Task WhenProcessingUnknownMessage_ThenNothingIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<object>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task WhenProcessingSqsRequestWithoutCorrespondingSqsLambdaRequest_ThenThrowsNotImplementedException()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<TestSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            var act = async () => await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotImplementedException>();
        }

        [Fact]
        public async Task WhenProcessingNotRealizeBuildingSqsRequest_ThenNotRealizeBuildingLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<NotRealizeBuildingSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<NotRealizeBuildingLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingCorrectBuildingNotRealizationSqsRequest_ThenCorrectBuildingNotRealizationLambdaRequestRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectBuildingNotRealizationSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectBuildingNotRealizationLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingPlaceBuildingUnderConstructionSqsRequest_ThenPlaceBuildingUnderConstructionLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<PlaceBuildingUnderConstructionSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<PlaceBuildingUnderConstructionLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingCorrectPlaceBuildingUnderConstructionSqsRequest_ThenCorrectPlaceBuildingUnderConstructionLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectPlaceBuildingUnderConstructionSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectPlaceBuildingUnderConstructionLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingPlanBuildingSqsRequest_ThenPlanBuildingLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<PlanBuildingSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<PlanBuildingLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == null &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingRealizeBuildingSqsRequest_ThenRealizeBuildingLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RealizeBuildingSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RealizeBuildingLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingCorrectBuildingRealizationSqsRequest_ThenCorrectBuildingRealizationLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectBuildingRealizationSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectBuildingRealizationLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingNotRealizeBuildingUnitSqsRequest_ThenNotRealizeBuildingUnitLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<NotRealizeBuildingUnitSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<NotRealizeBuildingUnitLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingPlanBuildingUnitSqsRequest_ThenPlanBuildingUnitLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<PlanBuildingUnitSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<PlanBuildingUnitLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == null &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingRealizeBuildingUnitSqsRequest_ThenRealizeBuildingUnitLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RealizeBuildingUnitSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RealizeBuildingUnitLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), CancellationToken.None), Times.Once);
        }
    }

    public class TestSqsRequest : SqsRequest
    { }
}
