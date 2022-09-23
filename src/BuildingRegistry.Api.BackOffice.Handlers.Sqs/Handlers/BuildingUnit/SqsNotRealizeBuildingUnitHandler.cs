namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Handlers.BuildingUnit
{
    using Abstractions;
    using Requests.Building;
    using System.Collections.Generic;
    using Requests.BuildingUnit;
    using TicketingService.Abstractions;

    public sealed class SqsNotRealizeBuildingUnitHandler : SqsHandler<SqsNotRealizeBuildingUnitRequest>
    {
        public const string Action = "NotRealizeBuildingUnit";
        private readonly BackOfficeContext _backOfficeContext;

        public SqsNotRealizeBuildingUnitHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(SqsNotRealizeBuildingUnitRequest request)
        {
            var relation = _backOfficeContext
                .BuildingUnitBuildings
                .Find(request.Request.BuildingUnitPersistentLocalId);

            return relation?.BuildingPersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SqsNotRealizeBuildingUnitRequest sqsRequest)
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
