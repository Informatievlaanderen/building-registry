namespace BuildingRegistry.Api.CrabImport.Handlers.Sqs
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Post;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;

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
            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            return Unit.Value;
        }
    }
}
