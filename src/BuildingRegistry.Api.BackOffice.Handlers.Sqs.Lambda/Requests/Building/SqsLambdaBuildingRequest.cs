namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests.Building
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using MediatR;
    using System;
    using System.Collections.Generic;

    public class SqsLambdaBuildingRequest : IRequest
    {
        public Guid TicketId { get; set; }
        public string MessageGroupId { get; set; }
        public string? IfMatchHeaderValue { get; set; }
        public Provenance Provenance { get; set; }
        public IDictionary<string, object> Metadata { get; set; }
    }
}
