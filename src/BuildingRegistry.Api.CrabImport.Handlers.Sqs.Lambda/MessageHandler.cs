namespace BuildingRegistry.Api.CrabImport.Handlers.Sqs.Lambda
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Post;
    using Be.Vlaanderen.Basisregisters.Aws.Lambda;
    using MediatR;

    public class MessageHandler : IMessageHandler
    {
        private readonly IMediator _mediator;

        public MessageHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task HandleMessage(object? messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken)
        {
            if (messageData is not SqsPostRequest sqsPostRequest)
            {
                return;
            }

            await _mediator.Send(sqsPostRequest, cancellationToken);
        }
    }
}
