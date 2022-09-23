namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests.BuildingUnit
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using MediatR;

    public class SqsLambdaBuildingUnitRequest : IRequest
    {
        public Guid TicketId { get; set; }
        public string MessageGroupId { get; set; }
        public string? IfMatchHeaderValue { get; set; }
        public Provenance Provenance { get; set; }
        public IDictionary<string, object> Metadata { get; set; }

        public BuildingPersistentLocalId BuildingPersistentLocalId =>
            new BuildingPersistentLocalId(Convert.ToInt32(MessageGroupId));
    }
}
