namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Building
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Sqs;
    using TicketingService.Abstractions;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;
    using static Microsoft.AspNetCore.Http.Results;

    public class SqsPlaceBuildingUnderConstructionHandler : IRequestHandler<SqsPlaceBuildingUnderConstructionRequest, IResult>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ITicketing _ticketing;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly ILogger<SqsPlaceBuildingUnderConstructionHandler> _logger;

        public SqsPlaceBuildingUnderConstructionHandler(
            SqsOptions sqsOptions,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            ILogger<SqsPlaceBuildingUnderConstructionHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _ticketing = ticketing;
            _ticketingUrl = ticketingUrl;
            _logger = logger;
        }

        public async Task<IResult> Handle(SqsPlaceBuildingUnderConstructionRequest request, CancellationToken cancellationToken)
        {
            var ticketId = await _ticketing.CreateTicket(nameof(BuildingRegistry), cancellationToken);
            request.TicketId = ticketId;

            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, new SqsQueueOptions(request.PersistentLocalId.ToString()), cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            var location = _ticketingUrl.For(request.TicketId);
            return Accepted(location);
        }
    }
}
