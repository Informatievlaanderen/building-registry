namespace BuildingRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building;
    using BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit;
    using BuildingRegistry.Building;
    using Fixtures;
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
                It.IsAny<CancellationToken>());

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
                It.IsAny<CancellationToken>());

            // Assert
            await act
                .Should()
                .ThrowAsync<NotImplementedException>()
                .WithMessage($"{nameof(TestSqsRequest)} has no corresponding SqsLambdaRequest defined.");
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
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<NotRealizeBuildingLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
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
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectBuildingNotRealizationLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
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
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<PlaceBuildingUnderConstructionLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
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
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectPlaceBuildingUnderConstructionLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
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
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<PlanBuildingLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == null &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingChangeBuildingOutlineSqsRequest_ThenChangeBuildingOutlineLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<ChangeBuildingOutlineSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<ChangeBuildingOutlineLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.BuildingPersistentLocalId == messageData.BuildingPersistentLocalId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
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
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RealizeBuildingLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
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
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectBuildingRealizationLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
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
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<NotRealizeBuildingUnitLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingCorrectBuildingUnitNotRealizationSqsRequest_ThenCorrectBuildingUnitNotRealizationLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectBuildingUnitNotRealizationSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectBuildingUnitNotRealizationLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
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
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<PlanBuildingUnitLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == null &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
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
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RealizeBuildingUnitLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task WhenProcessingRetireBuildingUnitSqsRequest_ThenRetireBuildingUnitLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RetireBuildingUnitSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RetireBuildingUnitLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingCorrectBuildingRetirementqsRequest_ThenCorrectBuildingRetirementLambdaRequestRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectBuildingUnitRetirementSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectBuildingUnitRetirementLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingCorrectBuildingUnitRemovalSqsRequest_ThenCorrectBuildingUnitRemovalLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectBuildingUnitRemovalSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectBuildingUnitRemovalLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingRemoveBuildingUnitSqsRequest_ThenRemoveBuildingUnitLambdaRequestRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RemoveBuildingUnitSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RemoveBuildingUnitLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingDeregulateBuildingUnitSqsRequest_ThenDeregulateBuildingUnitLambdaRequestRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<DeregulateBuildingUnitSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<DeregulateBuildingUnitLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingRegularizeBuildingUnitSqsRequest_ThenRegularizeBuildingUnitLambdaRequestRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RegularizeBuildingUnitSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RegularizeBuildingUnitLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingCorrectBuildingUnitRegularizationSqsRequest_ThenCorrectBuildingUnitRegularizationLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CorrectBuildingUnitRegularizationSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectBuildingUnitRegularizationLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingAttachAddressToBuildingUnitSqsRequest_ThenAttachAddressToBuildingUnitLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<AttachAddressToBuildingUnitSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<int>().ToString() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<AttachAddressToBuildingUnitLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.BuildingPersistentLocalId == int.Parse(messageMetadata.MessageGroupId) &&
                    request.BuildingUnitPersistentLocalId == messageData.BuildingUnitPersistentLocalId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingDetachAddressFromBuildingUnitSqsRequest_ThenDetachAddressFromBuildingUnitLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<DetachAddressFromBuildingUnitSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<int>().ToString() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<DetachAddressFromBuildingUnitLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.BuildingPersistentLocalId == int.Parse(messageMetadata.MessageGroupId) &&
                    request.BuildingUnitPersistentLocalId == messageData.BuildingUnitPersistentLocalId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingRemoveBuildingSqsRequest_ThenRemoveBuildingLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RemoveBuildingSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<int>().ToString() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RemoveBuildingLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.BuildingPersistentLocalId == messageData.Request.PersistentLocalId &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingRemoveMeasuredBuildingSqsRequest_ThenRemoveMeasuredBuildingLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<RemoveMeasuredBuildingSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<int>().ToString() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RemoveMeasuredBuildingLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    request.BuildingPersistentLocalId == messageData.Request.PersistentLocalId &&
                    request.IfMatchHeaderValue == messageData.IfMatchHeaderValue &&
                    request.Provenance == messageData.ProvenanceData!.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingRealizingAndMeasuringUnplannedBuildingSqsRequest_ThenLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var sqsRequest = Fixture.Create<RealizeAndMeasureUnplannedBuildingSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<int>().ToString() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                sqsRequest,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<RealizeAndMeasureUnplannedBuildingLambdaRequest>(request =>
                    request.TicketId == sqsRequest.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == sqsRequest.Request &&
                    request.BuildingPersistentLocalId == sqsRequest.BuildingPersistentLocalId &&
                    request.IfMatchHeaderValue == null &&
                    request.Provenance == sqsRequest.ProvenanceData.ToProvenance() &&
                    request.Metadata == sqsRequest.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingMeasureBuildingSqsRequest_ThenLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var sqsRequest = Fixture.Create<MeasureBuildingSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<int>().ToString() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                sqsRequest,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<MeasureBuildingLambdaRequest>(request =>
                    request.TicketId == sqsRequest.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == sqsRequest.Request &&
                    request.BuildingPersistentLocalId == sqsRequest.BuildingPersistentLocalId &&
                    request.IfMatchHeaderValue == null &&
                    request.Provenance == sqsRequest.ProvenanceData.ToProvenance() &&
                    request.Metadata == sqsRequest.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingCorrectBuildingMeasurementSqsRequest_ThenLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var sqsRequest = Fixture.Create<CorrectBuildingMeasurementSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<int>().ToString() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                sqsRequest,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CorrectBuildingMeasurementLambdaRequest>(request =>
                    request.TicketId == sqsRequest.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == sqsRequest.Request &&
                    request.BuildingPersistentLocalId == sqsRequest.BuildingPersistentLocalId &&
                    request.IfMatchHeaderValue == null &&
                    request.Provenance == sqsRequest.ProvenanceData.ToProvenance() &&
                    request.Metadata == sqsRequest.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingDemolishBuildingSqsRequest_ThenLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var sqsRequest = Fixture.Create<DemolishBuildingSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<int>().ToString() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                sqsRequest,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<DemolishBuildingLambdaRequest>(request =>
                    request.TicketId == sqsRequest.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == sqsRequest.Request &&
                    request.BuildingPersistentLocalId == sqsRequest.BuildingPersistentLocalId &&
                    request.IfMatchHeaderValue == sqsRequest.IfMatchHeaderValue &&
                    request.Provenance == sqsRequest.ProvenanceData.ToProvenance() &&
                    request.Metadata == sqsRequest.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingMoveBuildingUnitSqsRequest_ThenLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var sqsRequest = Fixture.Create<MoveBuildingUnitSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<int>().ToString() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                sqsRequest,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<MoveBuildingUnitLambdaRequest>(request =>
                    request.TicketId == sqsRequest.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == sqsRequest.Request &&
                    request.BuildingUnitPersistentLocalId == sqsRequest.BuildingUnitPersistentLocalId &&
                    request.IfMatchHeaderValue == sqsRequest.IfMatchHeaderValue &&
                    request.Provenance == sqsRequest.ProvenanceData.ToProvenance() &&
                    request.Metadata == sqsRequest.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingNotifyOutlinedRealizedBuildingSqsRequestSqsRequest_ThenLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            Fixture.Customize(new WithValidExtendedWkbPolygon());
            var messageData = new NotifyOutlinedRealizedBuildingSqsRequest(
                Fixture.Create<int>(),
                Fixture.Create<string>(),
                Fixture.Create<DateTimeOffset>(),
                Fixture.Create<ExtendedWkbGeometry>().ToString());
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                It.IsAny<CancellationToken>());

            // Assert
            mediator
                .Verify(x =>
                    x.Send(It.Is<NotifyOutlinedRealizedBuildingLambdaRequest>(request =>
                            request.BuildingPersistentLocalId == messageData.BuildingPersistentLocalId &&
                            request.Organisation == messageData.Organisation &&
                            request.ExtendedWkbGeometry == new ExtendedWkbGeometry(messageData.ExtendedWkbGeometry) &&
                            request.DateTimeStatusChange == messageData.DateTimeStatusChange),
                        It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingCreateBuildingOsloSnapshotsSqsRequest_ThenCreateBuildingOsloSnapshotsLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CreateBuildingOsloSnapshotsSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CreateBuildingOsloSnapshotsLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    string.IsNullOrEmpty(request.IfMatchHeaderValue) &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenProcessingCreateBuildingUnitOsloSnapshotsSqsRequest_ThenCreateBuildingUnitOsloSnapshotsLambdaRequestIsSent()
        {
            // Arrange
            var mediator = new Mock<IMediator>();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => mediator.Object);
            var container = containerBuilder.Build();

            var messageData = Fixture.Create<CreateBuildingUnitOsloSnapshotsSqsRequest>();
            var messageMetadata = new MessageMetadata { MessageGroupId = Fixture.Create<string>() };

            var sut = new MessageHandler(container);

            // Act
            await sut.HandleMessage(
                messageData,
                messageMetadata,
                CancellationToken.None);

            // Assert
            mediator
                .Verify(x => x.Send(It.Is<CreateBuildingUnitOsloSnapshotsLambdaRequest>(request =>
                    request.TicketId == messageData.TicketId &&
                    request.MessageGroupId == messageMetadata.MessageGroupId &&
                    request.Request == messageData.Request &&
                    string.IsNullOrEmpty(request.IfMatchHeaderValue) &&
                    request.Provenance == messageData.ProvenanceData.ToProvenance() &&
                    request.Metadata == messageData.Metadata
                ), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    internal class TestSqsRequest : SqsRequest
    { }
}
