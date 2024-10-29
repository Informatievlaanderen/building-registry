namespace BuildingRegistry.Api.BackOffice.Handlers.Building
{
    using System.Collections.Generic;
    using Abstractions.Building.SqsRequests;
    using AllStream;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class CreateBuildingOsloSnapshotsSqsHandler : SqsHandler<CreateBuildingOsloSnapshotsSqsRequest>
    {
        public const string Action = "CreateBuildingOsloSnapshots";

        public CreateBuildingOsloSnapshotsSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
        { }

        protected override string? WithAggregateId(CreateBuildingOsloSnapshotsSqsRequest request)
        {
            return AllStreamId.Instance;
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CreateBuildingOsloSnapshotsSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(BuildingRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId }
            };
        }
    }
}
