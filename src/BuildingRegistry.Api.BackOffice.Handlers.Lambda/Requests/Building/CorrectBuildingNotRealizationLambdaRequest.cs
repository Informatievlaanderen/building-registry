namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.Building;

    public sealed record CorrectBuildingNotRealizationLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<CorrectBuildingNotRealizationBackOfficeRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public CorrectBuildingNotRealizationBackOfficeRequest Request { get; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public CorrectBuildingNotRealizationLambdaRequest(
            string messageGroupId,
            CorrectBuildingNotRealizationSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public CorrectBuildingNotRealizationLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            CorrectBuildingNotRealizationBackOfficeRequest request)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        /// <summary>
        /// Map to CorrectBuildingNotRealization command
        /// </summary>
        /// <returns>CorrectBuildingNotRealization.</returns>
        public CorrectBuildingNotRealization ToCommand()
        {
            return new CorrectBuildingNotRealization(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
