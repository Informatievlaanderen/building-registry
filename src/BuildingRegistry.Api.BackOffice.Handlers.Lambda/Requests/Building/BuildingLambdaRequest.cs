namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;

    public record BuildingLambdaRequest : SqsLambdaRequest
    {
        public BuildingLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        { }
    }
}
