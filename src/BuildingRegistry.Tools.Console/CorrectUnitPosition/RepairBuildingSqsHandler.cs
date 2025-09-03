namespace BuildingRegistry.Tools.Console.CorrectUnitPosition
{
    using System.Collections.Generic;
    using Api.BackOffice.Abstractions.Building.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public class RepairBuildingSqsHandler : SqsHandler<RepairBuildingSqsRequest>
    {
        public const string Action = "RepairBuilding";

        public RepairBuildingSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
        }

        protected override string WithAggregateId(RepairBuildingSqsRequest request)
        {
            return request.BuildingPersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, RepairBuildingSqsRequest sqsRequest)
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
