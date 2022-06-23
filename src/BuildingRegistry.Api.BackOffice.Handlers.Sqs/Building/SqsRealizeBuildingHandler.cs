namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class SqsRealizeBuildingHandler : IRequestHandler<SqsRealizeBuildingRequest, Unit>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ILogger<SqsRealizeBuildingHandler> _logger;

        public SqsRealizeBuildingHandler(
            SqsOptions sqsOptions,
            ILogger<SqsRealizeBuildingHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsRealizeBuildingRequest request, CancellationToken cancellationToken)
        {
            var queueName = $"{nameof(BuildingRegistry)}.{nameof(Api)}.{nameof(BackOffice)}.{nameof(Building)}.{nameof(SqsRealizeBuildingHandler)}";
            var queueUrl = await SqsQueue.CreateQueue(_sqsOptions, queueName, true, cancellationToken);

            await SqsProducer.Produce(_sqsOptions, queueUrl, request, string.Empty, cancellationToken);

            _logger.LogDebug($"Request sent to queue {queueName}");

            return Unit.Value;
        }
    }
}
