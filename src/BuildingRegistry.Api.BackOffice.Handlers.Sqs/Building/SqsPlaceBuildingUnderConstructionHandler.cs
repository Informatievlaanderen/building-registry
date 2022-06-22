namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class SqsPlaceBuildingUnderConstructionHandler : IRequestHandler<SqsPlaceBuildingUnderConstructionRequest, Unit>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ILogger<SqsPlaceBuildingUnderConstructionHandler> _logger;

        public SqsPlaceBuildingUnderConstructionHandler(SqsOptions sqsOptions, ILogger<SqsPlaceBuildingUnderConstructionHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsPlaceBuildingUnderConstructionRequest request, CancellationToken cancellationToken)
        {
            var queueName = $"{nameof(BuildingRegistry)}.{nameof(Api)}.{nameof(BackOffice)}.{nameof(Building)}.{nameof(SqsPlaceBuildingUnderConstructionHandler)}";
            var queueUrl = await SqsQueue.CreateQueue(_sqsOptions, queueName, true, cancellationToken);

            await SqsProducer.Produce(_sqsOptions, queueUrl, request, string.Empty, cancellationToken);

            _logger.LogDebug($"Request sent to queue {queueName}");

            return Unit.Value;
        }
    }
}
