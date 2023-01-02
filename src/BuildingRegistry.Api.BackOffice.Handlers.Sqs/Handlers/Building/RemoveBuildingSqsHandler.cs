namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Handlers.Building
{
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class RemoveBuildingSqsHandler : SqsHandler<RemoveBuildingSqsRequest>
    {
        public const string Action = "RemoveBuilding";

        public RemoveBuildingSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
        }

        protected override string WithAggregateId(RemoveBuildingSqsRequest request)
        {
            return request.Request.PersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, RemoveBuildingSqsRequest sqsRequest)
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
