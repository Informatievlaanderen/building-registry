namespace BuildingRegistry.Api.BackOffice.Handlers.BuildingUnit
{
    using System.Collections.Generic;
    using Abstractions.BuildingUnit.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class PlanBuildingUnitSqsHandler : SqsHandler<PlanBuildingUnitSqsRequest>
    {
        public const string Action = "PlanBuildingUnit";

        public PlanBuildingUnitSqsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
        }

        protected override string WithAggregateId(PlanBuildingUnitSqsRequest request)
        {
            var identifier = request
                .Request
                .GebouwId
                .AsIdentifier()
                .Map(x => x);

            return identifier;
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, PlanBuildingUnitSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(BuildingRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId }
            };
        }
    }
}
