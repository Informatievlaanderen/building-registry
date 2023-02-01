namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building;
    using Abstractions.Building.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record ChangeBuildingOutlineLambdaRequest : BuildingLambdaRequest, Abstractions.IHasBuildingPersistentLocalId
    {
        public ChangeBuildingOutlineRequest Request { get; }

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
            ChangeBuildingOutlineRequest request)
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
