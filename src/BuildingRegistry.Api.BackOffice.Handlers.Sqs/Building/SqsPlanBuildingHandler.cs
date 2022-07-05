namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Sqs;
    using TicketingService.Abstractions;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;

    public class SqsPlanBuildingHandler : IRequestHandler<SqsPlanBuildingRequest, Unit>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ITicketing _ticketing;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;
        private readonly ILogger<SqsPlanBuildingHandler> _logger;

        public SqsPlanBuildingHandler(
            SqsOptions sqsOptions,
            ITicketing ticketing,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            ILogger<SqsPlanBuildingHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _ticketing = ticketing;
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsPlanBuildingRequest request, CancellationToken cancellationToken)
        {
            var ticketId = await _ticketing.CreateTicket(nameof(BuildingRegistry));
            request.TicketId = ticketId;
                
            var buildingPersistentLocalId = _persistentLocalIdGenerator.GenerateNextPersistentLocalId();
            request.MessageGroupId = buildingPersistentLocalId.ToString();
            
            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, request.MessageGroupId, cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            return Unit.Value;
        }
    }
}
