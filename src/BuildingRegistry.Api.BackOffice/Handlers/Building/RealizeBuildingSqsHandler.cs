namespace BuildingRegistry.Api.BackOffice.Handlers.Building
{
    using System.Collections.Generic;
    using Abstractions.Building.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class RealizeBuildingSqsHandler : SqsHandler<RealizeBuildingSqsRequest>
    {
        public const string Action = "RealizeBuilding";

        public RealizeBuildingSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
        }

        protected override string WithAggregateId(RealizeBuildingSqsRequest request)
        {
            return request.Request.PersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, RealizeBuildingSqsRequest sqsRequest)
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
