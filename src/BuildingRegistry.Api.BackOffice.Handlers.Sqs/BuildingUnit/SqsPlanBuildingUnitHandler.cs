namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.BuildingUnit.Requests;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;

    public class SqsPlanBuildingUnitHandler : IRequestHandler<SqsPlanBuildingUnitRequest, Unit>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ILogger<SqsPlanBuildingUnitHandler> _logger;

        public SqsPlanBuildingUnitHandler(SqsOptions sqsOptions, ILogger<SqsPlanBuildingUnitHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _logger = logger;
        }
        
        public async Task<Unit> Handle(SqsPlanBuildingUnitRequest request, CancellationToken cancellationToken)
        {
            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            return Unit.Value;
        }
    }
}