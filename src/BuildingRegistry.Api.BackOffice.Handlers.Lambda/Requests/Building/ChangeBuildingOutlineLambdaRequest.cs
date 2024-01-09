namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building;
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record ChangeBuildingOutlineLambdaRequest : BuildingLambdaRequest, Abstractions.IHasBuildingPersistentLocalId
    {
        public ChangeBuildingOutlineRequest Request { get; }

        public int BuildingPersistentLocalId { get; }

        public ChangeBuildingOutlineLambdaRequest(
            string messageGroupId,
            ChangeBuildingOutlineSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            BuildingPersistentLocalId = sqsRequest.BuildingPersistentLocalId;
            Request = sqsRequest.Request;
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
