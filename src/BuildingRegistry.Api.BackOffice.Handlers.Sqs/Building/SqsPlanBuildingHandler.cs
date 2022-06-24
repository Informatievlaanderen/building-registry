namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;

    public class SqsPlanBuildingHandler : IRequestHandler<SqsPlanBuildingRequest, Unit>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ILogger<SqsPlanBuildingHandler> _logger;

        public SqsPlanBuildingHandler(
            SqsOptions sqsOptions,
            ILogger<SqsPlanBuildingHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsPlanBuildingRequest request, CancellationToken cancellationToken)
        {
            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            return Unit.Value;
        }
    }
}
