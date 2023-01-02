namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Handlers.BuildingUnit
{
    using Abstractions;
    using Requests.BuildingUnit;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class AttachAddressToBuildingUnitSqsHandler : SqsHandler<AttachAddressToBuildingUnitSqsRequest>
    {
        public const string Action = "AttachAddressToBuildingUnit";
        private readonly BackOfficeContext _backOfficeContext;

        public AttachAddressToBuildingUnitSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(AttachAddressToBuildingUnitSqsRequest request)
        {
            var relation = _backOfficeContext
                .BuildingUnitBuildings
                .Find(request.BuildingUnitPersistentLocalId);

            return relation?.BuildingPersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, AttachAddressToBuildingUnitSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(BuildingRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId },
                { ObjectIdKey, sqsRequest.BuildingUnitPersistentLocalId.ToString() }
            };
        }
    }
}