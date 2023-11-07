namespace BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit
{
    using System.Collections.Generic;
    using Abstractions;
    using Abstractions.BuildingUnit.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class DetachAddressFromBuildingUnitSqsHandler : SqsHandler<DetachAddressFromBuildingUnitSqsRequest>
    {
        public const string Action = "DetachAddressBuildingUnit";
        private readonly BackOfficeContext _backOfficeContext;

        public DetachAddressFromBuildingUnitSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(DetachAddressFromBuildingUnitSqsRequest request)
        {
            var relation = _backOfficeContext
                .BuildingUnitBuildings
                .Find(request.BuildingUnitPersistentLocalId);

            return relation?.BuildingPersistentLocalId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, DetachAddressFromBuildingUnitSqsRequest sqsRequest)
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
