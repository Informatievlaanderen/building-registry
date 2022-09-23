namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Handlers.Building
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building;
    using Sqs;
    using System.Collections.Generic;
    using TicketingService.Abstractions;

    public sealed class SqsPlanBuildingHandler : SqsHandler<SqsPlanBuildingRequest>
    {
        public const string Action = "PlanBuilding";
        private readonly BackOfficeContext _backOfficeContext;

        public SqsPlanBuildingHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(SqsPlanBuildingRequest request)
        {
            return "0";
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SqsPlanBuildingRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(BuildingRegistry) },
                { ActionKey, Action }
            };
        }
    }
}
