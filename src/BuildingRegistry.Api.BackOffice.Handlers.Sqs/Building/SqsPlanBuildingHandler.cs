namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Sqs;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;

    public class SqsPlanBuildingHandler : IRequestHandler<SqsPlanBuildingRequest, Unit>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;
        private readonly ILogger<SqsPlanBuildingHandler> _logger;

        public SqsPlanBuildingHandler(
            SqsOptions sqsOptions,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            ILogger<SqsPlanBuildingHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsPlanBuildingRequest request, CancellationToken cancellationToken)
        {
            var persistentLocalId = _persistentLocalIdGenerator.GenerateNextPersistentLocalId();
            request.MessageGroupId = persistentLocalId.ToString();
            
            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, request.MessageGroupId, cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            return Unit.Value;
        }
    }
}
