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
                    await mediator.Send(new NotRealizeBuildingLambdaRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        IfMatchHeaderValue = request.IfMatchHeaderValue,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case PlaceBuildingUnderConstructionSqsRequest request:
                    await mediator.Send(new PlaceBuildingUnderConstructionLambdaRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        IfMatchHeaderValue = request.IfMatchHeaderValue,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case PlanBuildingSqsRequest request:
                    await mediator.Send(new PlanBuildingLambdaRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case RealizeBuildingSqsRequest request:
                    await mediator.Send(new RealizeBuildingLambdaRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        IfMatchHeaderValue = request.IfMatchHeaderValue,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case CorrectBuildingRealizationSqsRequest request:
                    await mediator.Send(new CorrectBuildingRealizationLambdaRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        IfMatchHeaderValue = request.IfMatchHeaderValue,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case NotRealizeBuildingUnitSqsRequest request:
                    await mediator.Send(new NotRealizeBuildingUnitLambdaRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        IfMatchHeaderValue = request.IfMatchHeaderValue,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case PlanBuildingUnitSqsRequest request:
                    await mediator.Send(new PlanBuildingUnitLambdaRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                case RealizeBuildingUnitSqsRequest request:
                    await mediator.Send(new RealizeBuildingUnitLambdaRequest
                    {
                        Request = request.Request,
                        TicketId = request.TicketId,
                        MessageGroupId = messageMetadata.MessageGroupId!,
                        IfMatchHeaderValue = request.IfMatchHeaderValue,
                        Metadata = request.Metadata,
                        Provenance = request.ProvenanceData.ToProvenance()
                    }, cancellationToken);
                    break;

                default:
                    throw new NotImplementedException(
                        $"{sqsRequest.GetType().Name} has no corresponding SqsLambdaRequest defined.");
            }
        }
    }
}
