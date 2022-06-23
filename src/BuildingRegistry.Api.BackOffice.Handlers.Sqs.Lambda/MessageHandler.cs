namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using MediatR;

    public class MessageHandler : IMessageHandler
    {
        private readonly IMediator _mediator;

        public MessageHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task HandleMessage(object? messageData, CancellationToken cancellationToken)
        {
            switch (messageData)
            {
                case SqsPlanBuildingRequest sqsPlanBuildingRequest:
                    await _mediator.Send(sqsPlanBuildingRequest, cancellationToken);
                    break;

                case SqsPlaceBuildingUnderConstructionRequest sqsPlaceBuildingUnderConstructionRequest:
                    await _mediator.Send(sqsPlaceBuildingUnderConstructionRequest, cancellationToken);
                    break;

                case SqsRealizeBuildingRequest sqsRealizeBuildingRequest:
                    await _mediator.Send(sqsRealizeBuildingRequest, cancellationToken);
                    break;
            }
        }
    }
}
