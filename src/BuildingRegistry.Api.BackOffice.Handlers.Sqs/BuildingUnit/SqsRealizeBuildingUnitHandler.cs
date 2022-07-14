namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.BuildingUnit
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.BuildingUnit.Extensions;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using TicketingService.Abstractions;
    using static Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Sqs;
    using static Microsoft.AspNetCore.Http.Results;


    public class SqsRealizeBuildingUnitHandler : IRequestHandler<SqsRealizeBuildingUnitRequest, IResult>
    {
        private readonly SqsOptions _sqsOptions;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly BackOfficeContext _backOfficeContext;
        private readonly ILogger<SqsPlanBuildingUnitHandler> _logger;

        public SqsRealizeBuildingUnitHandler(
            SqsOptions sqsOptions,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext,
            ILogger<SqsPlanBuildingUnitHandler> logger)
        {
            _sqsOptions = sqsOptions;
            _ticketingUrl = ticketingUrl;
            _backOfficeContext = backOfficeContext;
            _logger = logger;
        }

        public async Task<IResult> Handle(SqsRealizeBuildingUnitRequest request, CancellationToken cancellationToken)
        {
            if (!request.PersistentLocalId.TryGetBuildingIdForBuildingUnit(_backOfficeContext, out var buildingPersistentLocalId))
            {
                throw new InvalidOperationException();
            }

            request.MessageGroupId = buildingPersistentLocalId.ToString();

            _ = await CopyToQueue(_sqsOptions, SqsQueueName.Value, request, new SqsQueueOptions(request.MessageGroupId), cancellationToken);

            _logger.LogDebug($"Request sent to queue {SqsQueueName.Value}");

            var location = _ticketingUrl.For(request.TicketId);
            return Accepted(location);
        }
    }
}
