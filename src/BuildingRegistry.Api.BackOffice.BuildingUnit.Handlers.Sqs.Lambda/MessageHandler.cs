namespace BuildingRegistry.Api.BackOffice.BuildingUnit.Handlers.Sqs.Lambda
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
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
                case SqsPlanBuildingUnitRequest sqsPlanBuildingUnitRequest:
                    await _mediator.Send(sqsPlanBuildingUnitRequest, cancellationToken);
                    break;
            }
        }
    }
}
