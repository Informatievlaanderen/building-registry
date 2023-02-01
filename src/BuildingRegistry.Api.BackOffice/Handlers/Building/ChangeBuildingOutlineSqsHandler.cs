namespace BuildingRegistry.Api.BackOffice.Handlers.Building
{
    using System.Collections.Generic;
    using Abstractions.Building.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class ChangeBuildingOutlineSqsHandler : SqsHandler<ChangeBuildingOutlineSqsRequest>
    {
        public const string Action = "ChangeBuildingOutline";

        public ChangeBuildingOutlineSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        { }

        protected override string WithAggregateId(ChangeBuildingOutlineSqsRequest request)
        {
            return request.BuildingPersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeBuildingOutlineSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(BuildingRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId },
                { ObjectIdKey, sqsRequest.BuildingPersistentLocalId.ToString() }
            };
        }
    }
}
