namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Handlers.Building
{
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building;
    using Sqs;
    using System.Collections.Generic;
    using TicketingService.Abstractions;

    public sealed class PlaceBuildingUnderConstructionSqsHandler : SqsHandler<PlaceBuildingUnderConstructionSqsRequest>
    {
        public const string Action = "PlaceBuildingUnderConstruction";

        public PlaceBuildingUnderConstructionSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
        }

        protected override string WithAggregateId(PlaceBuildingUnderConstructionSqsRequest request)
        {
            return request.Request.PersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, PlaceBuildingUnderConstructionSqsRequest sqsRequest)
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
