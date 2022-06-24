namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;

    public class SqsPlaceBuildingUnderConstructionHandler : IRequestHandler<SqsPlaceBuildingUnderConstructionRequest, Unit>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ILogger<SqsPlaceBuildingUnderConstructionHandler> _logger;

        public SqsPlaceBuildingUnderConstructionHandler(
            SqsOptions sqsOptions,
            ILogger<SqsPlaceBuildingUnderConstructionHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsPlaceBuildingUnderConstructionRequest request, CancellationToken cancellationToken)
        {
            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            return Unit.Value;
        }
    }
}
