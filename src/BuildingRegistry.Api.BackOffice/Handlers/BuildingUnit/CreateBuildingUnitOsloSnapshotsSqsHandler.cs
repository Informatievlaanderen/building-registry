namespace BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit
{
    using System.Collections.Generic;
    using AllStream;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using TicketingService.Abstractions;

    public sealed class CreateBuildingUnitOsloSnapshotsSqsHandler : SqsHandler<CreateBuildingUnitOsloSnapshotsSqsRequest>
    {
        public const string Action = "CreateBuildingUnitOsloSnapshots";

        public CreateBuildingUnitOsloSnapshotsSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
        { }

        protected override string? WithAggregateId(CreateBuildingUnitOsloSnapshotsSqsRequest request)
        {
            return AllStreamId.Instance;
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CreateBuildingUnitOsloSnapshotsSqsRequest sqsRequest)
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
