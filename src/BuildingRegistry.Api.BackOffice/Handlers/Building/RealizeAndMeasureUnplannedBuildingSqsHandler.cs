namespace BuildingRegistry.Api.BackOffice.Handlers.Building
{
    using System.Collections.Generic;
    using Abstractions.Building.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class RealizeAndMeasureUnplannedBuildingSqsHandler : SqsHandler<RealizeAndMeasureUnplannedBuildingSqsRequest>
    {
        public const string Action = "RealizeAndMeasureUnplannedBuilding";

        public RealizeAndMeasureUnplannedBuildingSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
        }

        protected override string WithAggregateId(RealizeAndMeasureUnplannedBuildingSqsRequest request)
        {
            return request.BuildingPersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, RealizeAndMeasureUnplannedBuildingSqsRequest sqsRequest)
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
