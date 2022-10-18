namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using MediatR;
    using Requests.Building;
    using Requests.BuildingUnit;
    using Sqs.Requests;
    using Sqs.Requests.Building;
    using Sqs.Requests.BuildingUnit;

    public class MessageHandler : IMessageHandler
    {
        private readonly ILifetimeScope _container;

        public MessageHandler(ILifetimeScope container)
        {
            _container = container;
        }

        public async Task HandleMessage(object? messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken)
        {
            messageMetadata.Logger?.LogInformation($"Handling message {messageData?.GetType().Name}");

            if (messageData is not SqsRequest sqsRequest)
            {
                messageMetadata.Logger?.LogInformation($"Unable to cast {nameof(messageData)} as {nameof(sqsRequest)}.");
                return;
            }

            await using var lifetimeScope = _container.BeginLifetimeScope();
            var mediator = lifetimeScope.Resolve<IMediator>();

            switch (sqsRequest)
            {
                case NotRealizeBuildingSqsRequest request:
                    await mediator.Send(
                        new NotRealizeBuildingLambdaRequest(
                            request.TicketId,
                            messageMetadata.MessageGroupId!,
                            request.IfMatchHeaderValue,
                            request.ProvenanceData.ToProvenance(),
                            request.Metadata,
                            request.Request),
                        cancellationToken);
                    break;

                case CorrectBuildingNotRealizationSqsRequest request:
                    await mediator.Send(
                        new CorrectBuildingNotRealizationLambdaRequest(
                            request.TicketId,
                            messageMetadata.MessageGroupId!,
                            request.IfMatchHeaderValue,
                            request.ProvenanceData.ToProvenance(),
                            request.Metadata,
                            request.Request),
                        cancellationToken);
                    break;

                case PlaceBuildingUnderConstructionSqsRequest request:
                    await mediator.Send(
                        new PlaceBuildingUnderConstructionLambdaRequest(
                            request.TicketId,
                            messageMetadata.MessageGroupId!,
                            request.IfMatchHeaderValue,
                            request.ProvenanceData.ToProvenance(),
                            request.Metadata,
                            request.Request),
                        cancellationToken);
                    break;

                case CorrectPlaceBuildingUnderConstructionSqsRequest request:
                    await mediator.Send(
                        new CorrectPlaceBuildingUnderConstructionLambdaRequest(
                            request.TicketId,
                            messageMetadata.MessageGroupId!,
                            request.IfMatchHeaderValue,
                            request.ProvenanceData.ToProvenance(),
                            request.Metadata,
                            request.Request),
                        cancellationToken);
                    break;

                case PlanBuildingSqsRequest request:
                    await mediator.Send(
                        new PlanBuildingLambdaRequest(
                            request.TicketId,
                            messageMetadata.MessageGroupId!,
                            request.ProvenanceData.ToProvenance(),
                            request.Metadata,
                            request.Request),
                        cancellationToken);
                    break;

                case RealizeBuildingSqsRequest request:
                    await mediator.Send(
                        new RealizeBuildingLambdaRequest(
                            request.TicketId,
                            messageMetadata.MessageGroupId!,
                            request.IfMatchHeaderValue,
                            request.ProvenanceData.ToProvenance(),
                            request.Metadata,
                            request.Request
                            ),
                        cancellationToken);
                    break;

                case CorrectBuildingUnitRealizationSqsRequest request:
                    await mediator.Send(new CorrectBuildingUnitRealizationLambdaRequest
                    (
                        request.TicketId,
                        messageMetadata.MessageGroupId!,
                        request.IfMatchHeaderValue,
                        request.ProvenanceData.ToProvenance(),
                        request.Metadata,
                        request.Request),
                        cancellationToken);
                    break;

                case CorrectBuildingRealizationSqsRequest request:
                    await mediator.Send(
                        new CorrectBuildingRealizationLambdaRequest(
                            request.TicketId,
                            messageMetadata.MessageGroupId!,
                            request.IfMatchHeaderValue,
                            request.ProvenanceData.ToProvenance(),
                            request.Metadata,
                            request.Request),
                        cancellationToken);
                    break;

                case NotRealizeBuildingUnitSqsRequest request:
                    await mediator.Send(new NotRealizeBuildingUnitLambdaRequest(
                        request.TicketId,
                        messageMetadata.MessageGroupId!,
                        request.IfMatchHeaderValue,
                        request.ProvenanceData.ToProvenance(),
                        request.Metadata,
                        request.Request
                    ), cancellationToken);
                    break;

                case CorrectBuildingUnitNotRealizationSqsRequest request:
                    await mediator.Send(
                        new CorrectBuildingUnitNotRealizationLambdaRequest(
                            request.TicketId,
                            messageMetadata.MessageGroupId!,
                            request.IfMatchHeaderValue,
                            request.ProvenanceData.ToProvenance(),
                            request.Metadata,
                            request.Request),
                        cancellationToken);
                    break;

                case PlanBuildingUnitSqsRequest request:
                    await mediator.Send(new PlanBuildingUnitLambdaRequest(
                        request.TicketId,
                        messageMetadata.MessageGroupId!,
                        request.ProvenanceData.ToProvenance(),
                        request.Metadata,
                        request.Request
                    ), cancellationToken);
                    break;

                case RealizeBuildingUnitSqsRequest request:
                    await mediator.Send(new RealizeBuildingUnitLambdaRequest
                    (
                        request.TicketId,
                        messageMetadata.MessageGroupId!,
                        request.IfMatchHeaderValue,
                        request.ProvenanceData.ToProvenance(),
                        request.Metadata,
                        request.Request
                    ), cancellationToken);
                    break;

                default:
                    throw new NotImplementedException(
                        $"{sqsRequest.GetType().Name} has no corresponding SqsLambdaRequest defined.");
            }
        }
    }
}
