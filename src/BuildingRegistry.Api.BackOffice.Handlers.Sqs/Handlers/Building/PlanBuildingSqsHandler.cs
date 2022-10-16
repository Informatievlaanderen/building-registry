namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Handlers.Building
{
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class PlanBuildingSqsHandler : SqsHandler<PlanBuildingSqsRequest>
    {
        public const string Action = "PlanBuilding";

        public PlanBuildingSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
        }

        protected override string WithAggregateId(PlanBuildingSqsRequest request)
        {
            return "0";
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, PlanBuildingSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(BuildingRegistry) },
                { ActionKey, Action }
            };
        }
    }
}
