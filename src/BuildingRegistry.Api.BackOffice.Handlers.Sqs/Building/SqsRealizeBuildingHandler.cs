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

    public class SqsRealizeBuildingHandler : IRequestHandler<SqsRealizeBuildingRequest, Unit>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ITicketing _ticketing;
        private readonly ILogger<SqsRealizeBuildingHandler> _logger;

        public SqsRealizeBuildingHandler(
            SqsOptions sqsOptions,
            ITicketing ticketing,
            ILogger<SqsRealizeBuildingHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _ticketing = ticketing;
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsRealizeBuildingRequest request, CancellationToken cancellationToken)
        {
            var ticketId = await _ticketing.CreateTicket(nameof(BuildingRegistry));
            request.TicketId = ticketId;
                
            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, request.PersistentLocalId.ToString(), cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            return Unit.Value;
        }
    }
}
