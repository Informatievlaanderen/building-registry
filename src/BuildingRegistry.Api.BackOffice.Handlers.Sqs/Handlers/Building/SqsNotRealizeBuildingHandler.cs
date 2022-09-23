namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Handlers.Building
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building;
    using Sqs;
    using System.Collections.Generic;
    using TicketingService.Abstractions;

    public sealed class SqsNotRealizeBuildingHandler : SqsHandler<SqsNotRealizeBuildingRequest>
    {
        public const string Action = "NotRealizeBuilding";
        private readonly BackOfficeContext _backOfficeContext;

        public SqsNotRealizeBuildingHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(SqsNotRealizeBuildingRequest request)
        {
            var relation = _backOfficeContext
                .BuildingUnitBuildings
                .Find(request.Request.PersistentLocalId);

            return relation?.BuildingPersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SqsNotRealizeBuildingRequest sqsRequest)
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
