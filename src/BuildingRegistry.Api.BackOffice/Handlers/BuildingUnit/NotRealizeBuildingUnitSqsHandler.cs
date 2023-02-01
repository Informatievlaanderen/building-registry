namespace BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using TicketingService.Abstractions;

    public sealed class NotRealizeBuildingUnitSqsHandler : SqsHandler<NotRealizeBuildingUnitSqsRequest>
    {
        public const string Action = "NotRealizeBuildingUnit";
        private readonly BackOfficeContext _backOfficeContext;

        public NotRealizeBuildingUnitSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(NotRealizeBuildingUnitSqsRequest request)
        {
            var relation = _backOfficeContext
                .BuildingUnitBuildings
                .Find(request.Request.BuildingUnitPersistentLocalId);

            return relation?.BuildingPersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, NotRealizeBuildingUnitSqsRequest sqsRequest)
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
