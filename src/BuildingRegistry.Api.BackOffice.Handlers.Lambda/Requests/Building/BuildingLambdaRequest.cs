namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using MediatR;

    public class BuildingLambdaRequest : IRequest
    {
        public Guid TicketId { get; set; }
        public string MessageGroupId { get; set; }
        public string? IfMatchHeaderValue { get; set; }
        public Provenance Provenance { get; set; }
        public IDictionary<string, object> Metadata { get; set; }
    }
}
