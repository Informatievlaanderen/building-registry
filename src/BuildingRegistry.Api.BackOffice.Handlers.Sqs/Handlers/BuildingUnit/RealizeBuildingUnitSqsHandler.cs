namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Handlers.BuildingUnit
{
    using Abstractions;
    using Requests.BuildingUnit;
    using System.Collections.Generic;
    using TicketingService.Abstractions;

    public sealed class RealizeBuildingUnitSqsHandler : SqsHandler<RealizeBuildingUnitSqsRequest>
    {
        public const string Action = "RealizeBuildingUnit";
        private readonly BackOfficeContext _backOfficeContext;

        public RealizeBuildingUnitSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(RealizeBuildingUnitSqsRequest request)
        {
            var relation = _backOfficeContext
                .BuildingUnitBuildings
                .Find(request.Request.BuildingUnitPersistentLocalId);

            return relation?.BuildingPersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, RealizeBuildingUnitSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(BuildingRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId },
                { ObjectIdKey, sqsRequest.Request.BuildingUnitPersistentLocalId.ToString() }
            };
        }
    }
}
