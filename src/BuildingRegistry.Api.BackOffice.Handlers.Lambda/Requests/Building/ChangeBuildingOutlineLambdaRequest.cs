namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Abstractions.Building;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.Building;

    public sealed record ChangeBuildingOutlineLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<ChangeBuildingOutlineBackOfficeRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public ChangeBuildingOutlineBackOfficeRequest Request { get; }

        public int BuildingPersistentLocalId { get; }

        public ChangeBuildingOutlineLambdaRequest(
            string messageGroupId,
            ChangeBuildingOutlineSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.BuildingPersistentLocalId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public ChangeBuildingOutlineLambdaRequest(
            string messageGroupId,
            int buildingPersistentLocalId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            ChangeBuildingOutlineBackOfficeRequest request)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        {
            BuildingPersistentLocalId = buildingPersistentLocalId;
            Request = request;
        }

        /// <summary>
        /// Map to ChangeBuildingOutline command
        /// </summary>
        /// <returns>ChangeBuildingOutline.</returns>
        public ChangeBuildingOutline ToCommand()
        {
            return new ChangeBuildingOutline(
                new BuildingPersistentLocalId(BuildingPersistentLocalId),
                Request.GeometriePolygoon.ToExtendedWkbGeometry(),
                Provenance);
        }
    }
}