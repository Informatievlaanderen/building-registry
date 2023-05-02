namespace BuildingRegistry.Api.BackOffice.Handlers.Building
{
    using System.Collections.Generic;
    using Abstractions.Building.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class ChangeBuildingMeasurementSqsHandler : SqsHandler<ChangeBuildingMeasurementSqsRequest>
    {
        public const string Action = "ChangeBuildingMeasurement";

        public ChangeBuildingMeasurementSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        { }

        protected override string WithAggregateId(ChangeBuildingMeasurementSqsRequest request)
        {
            return request.BuildingPersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, ChangeBuildingMeasurementSqsRequest sqsRequest)
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
