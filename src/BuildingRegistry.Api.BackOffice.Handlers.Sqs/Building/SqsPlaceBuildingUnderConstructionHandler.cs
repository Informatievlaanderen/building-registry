namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Sqs;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;

    public class SqsPlaceBuildingUnderConstructionHandler : IRequestHandler<SqsPlaceBuildingUnderConstructionRequest, Unit>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ITicketing _ticketing;
        private readonly ILogger<SqsPlaceBuildingUnderConstructionHandler> _logger;

        public SqsPlaceBuildingUnderConstructionHandler(
            SqsOptions sqsOptions,
            ITicketing ticketing,
            ILogger<SqsPlaceBuildingUnderConstructionHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _ticketing = ticketing;
            _logger = logger;
        }

        public async Task<Unit> Handle(SqsPlaceBuildingUnderConstructionRequest request, CancellationToken cancellationToken)
        {
            var ticketId = await _ticketing.CreateTicket();
            request.TicketId = ticketId;

            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, request.PersistentLocalId.ToString(), cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            return Unit.Value;
        }
    }
}
