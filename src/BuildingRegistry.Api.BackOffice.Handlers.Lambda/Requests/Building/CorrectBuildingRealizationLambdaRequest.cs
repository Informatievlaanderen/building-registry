namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.Building;

    public sealed record CorrectBuildingRealizationLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<CorrectBuildingRealizationBackOfficeRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public CorrectBuildingRealizationBackOfficeRequest Request { get; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public CorrectBuildingRealizationLambdaRequest(
            string messageGroupId,
            CorrectBuildingRealizationSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public CorrectBuildingRealizationLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            CorrectBuildingRealizationBackOfficeRequest request)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        /// <summary>
        /// Map to CorrectBuildingRealization command
        /// </summary>
        /// <returns>CorrectBuildingRealization.</returns>
        public CorrectBuildingRealization ToCommand()
        {
            return new CorrectBuildingRealization(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
