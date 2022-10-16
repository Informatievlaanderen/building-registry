namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.Building;

    public sealed record PlaceBuildingUnderConstructionLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<PlaceBuildingUnderConstructionBackOfficeRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public PlaceBuildingUnderConstructionBackOfficeRequest Request { get; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public PlaceBuildingUnderConstructionLambdaRequest(
            string messageGroupId,
            PlaceBuildingUnderConstructionSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public PlaceBuildingUnderConstructionLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            PlaceBuildingUnderConstructionBackOfficeRequest request)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        /// <summary>
        /// Map to PlaceBuildingUnderConstruction command.
        /// </summary>
        /// <returns>PlaceBuildingUnderConstruction.</returns>
        public PlaceBuildingUnderConstruction ToCommand()
        {
            return new PlaceBuildingUnderConstruction(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
