namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.BuildingUnit
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.BuildingUnit.Requests;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using TicketingService.Abstractions;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;
    using static Microsoft.AspNetCore.Http.Results;

    public class SqsPlanBuildingUnitHandler : IRequestHandler<SqsPlanBuildingUnitRequest, IResult>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;
        private readonly ILogger<SqsPlanBuildingUnitHandler> _logger;

        public SqsPlanBuildingUnitHandler(
            SqsOptions sqsOptions,
            ITicketingUrl ticketingUrl,
            IPersistentLocalIdGenerator persistentLocalIdGenerator,
            ILogger<SqsPlanBuildingUnitHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _ticketingUrl = ticketingUrl;
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
            _logger = logger;
        }
        
        public async Task<IResult> Handle(SqsPlanBuildingUnitRequest request, CancellationToken cancellationToken)
        {
            var buildingPersistentLocalId = _persistentLocalIdGenerator.GenerateNextPersistentLocalId();
            request.MessageGroupId = buildingPersistentLocalId.ToString();

            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, new SqsQueueOptions(request.MessageGroupId), cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            var location = _ticketingUrl.For(request.TicketId);
            return Accepted(location);
        }
    }
}
