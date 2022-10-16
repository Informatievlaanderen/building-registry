namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Handlers.Building
{
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class CorrectBuildingRealizationSqsHandler : SqsHandler<CorrectBuildingRealizationSqsRequest>
    {
        public const string Action = "CorrectBuildingRealization";

        public CorrectBuildingRealizationSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
        }

        protected override string WithAggregateId(CorrectBuildingRealizationSqsRequest request)
        {
            return request.Request.PersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CorrectBuildingRealizationSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(BuildingRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId },
                { ObjectIdKey, sqsRequest.Request.PersistentLocalId.ToString() }
            };
        }
    }
}
