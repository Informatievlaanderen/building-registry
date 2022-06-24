namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;

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
            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            return Unit.Value;
        }
    }
}
