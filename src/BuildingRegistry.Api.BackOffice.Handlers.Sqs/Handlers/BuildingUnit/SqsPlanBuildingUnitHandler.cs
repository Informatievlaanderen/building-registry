namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Handlers.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using Requests.BuildingUnit;
    using System.Collections.Generic;
    using TicketingService.Abstractions;

    public sealed class SqsPlanBuildingUnitHandler : SqsHandler<SqsPlanBuildingUnitRequest>
    {
        public const string Action = "PlanBuildingUnit";
        private readonly BackOfficeContext _backOfficeContext;

        public SqsPlanBuildingUnitHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl,
            BackOfficeContext backOfficeContext)
            : base(sqsQueue, ticketing, ticketingUrl)
        {
            _backOfficeContext = backOfficeContext;
        }

        protected override string? WithAggregateId(SqsPlanBuildingUnitRequest request)
        {
            var identifier = request
                .Request
                .GebouwId
                .AsIdentifier()
                .Map(x => x);

            return identifier;
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, SqsPlanBuildingUnitRequest sqsRequest)
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
