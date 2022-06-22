namespace BuildingRegistry.Api.CrabImport.Handlers.Sqs
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Post;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class SqsPostHandler : IRequestHandler<SqsPostRequest, Unit>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ILogger<SqsPostHandler> _logger;

        public SqsPostHandler(SqsOptions sqsOptions, ILogger<SqsPostHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsPostRequest request, CancellationToken cancellationToken)
        {
            var queueName = $"{nameof(BuildingRegistry)}.{nameof(Api)}.{nameof(CrabImport)}.{nameof(SqsPostHandler)}";
            var queueUrl = await SqsQueue.CreateQueue(_sqsOptions, queueName, true, cancellationToken);

            await SqsProducer.Produce(_sqsOptions, queueUrl, request, string.Empty, cancellationToken);

            _logger.LogDebug($"Request sent to queue {queueName}");

            return Unit.Value;
        }
    }
}
