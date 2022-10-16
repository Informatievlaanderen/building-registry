namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.Building;

    public sealed record CorrectPlaceBuildingUnderConstructionLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<CorrectPlaceBuildingUnderConstructionBackOfficeRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public CorrectPlaceBuildingUnderConstructionBackOfficeRequest Request { get; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public CorrectPlaceBuildingUnderConstructionLambdaRequest(
            string messageGroupId,
            CorrectPlaceBuildingUnderConstructionSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public CorrectPlaceBuildingUnderConstructionLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            CorrectPlaceBuildingUnderConstructionBackOfficeRequest request)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        /// <summary>
        /// Map to CorrectBuildingPlaceUnderConstruction command.
        /// </summary>
        /// <returns>CorrectBuildingPlaceUnderConstruction.</returns>
        public CorrectBuildingPlaceUnderConstruction ToCommand()
        {
            return new CorrectBuildingPlaceUnderConstruction(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
