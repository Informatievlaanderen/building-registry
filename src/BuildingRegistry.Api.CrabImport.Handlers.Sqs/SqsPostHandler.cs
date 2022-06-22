namespace BuildingRegistry.Api.CrabImport.Handlers.Sqs
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Post;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class SqsPostHandler : IRequestHandler<SqsPostRequest, Unit>
    {
        private readonly ILogger<SqsPostHandler> _logger;

        public SqsPostHandler(ILogger<SqsPostHandler> logger)
        {
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsPostRequest request, CancellationToken cancellationToken)
        {
            var sqsOptions = new SqsOptions();
            var queueName = $"{nameof(BuildingRegistry)}.{nameof(Api)}.{nameof(CrabImport)}.{nameof(SqsPostHandler)}";
            var queueUrl = await SqsQueue.CreateQueue(sqsOptions, queueName, true, cancellationToken);

            await SqsProducer.Produce(sqsOptions, queueUrl, request, string.Empty, cancellationToken);

            _logger.LogDebug($"Request sent to queue {queueName}");

            return Unit.Value;
        }
    }
}
