namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class SqsPlanBuildingHandler : IRequestHandler<SqsPlanBuildingRequest, Unit>
    {
        private readonly ILogger<SqsPlanBuildingHandler> _logger;

        public SqsPlanBuildingHandler(ILogger<SqsPlanBuildingHandler> logger)
        {
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsPlanBuildingRequest request, CancellationToken cancellationToken)
        {
            var sqsOptions = new SqsOptions();
            var queueName = $"{nameof(BuildingRegistry)}.{nameof(Api)}.{nameof(BackOffice)}.{nameof(Building)}.{nameof(SqsPlanBuildingHandler)}";
            var queueUrl = await SqsQueue.CreateQueue(sqsOptions, queueName, true, cancellationToken);

            await SqsProducer.Produce(sqsOptions, queueUrl, request, string.Empty, cancellationToken);

            _logger.LogDebug($"Request sent to queue {queueName}");

            return Unit.Value;
        }
    }
}
