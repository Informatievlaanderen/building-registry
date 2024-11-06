namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.Requests;
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using IHasBuildingPersistentLocalId = Abstractions.IHasBuildingPersistentLocalId;

    public sealed record CreateBuildingSnapshotLambdaRequest : BuildingLambdaRequest, IHasBuildingPersistentLocalId
    {
        public CreateBuildingSnapshotRequest Request { get; }
        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public CreateBuildingSnapshotLambdaRequest(
            string messageGroupId,
            CreateBuildingSnapshotSqsRequest sqsRequest)
            : base(
                messageGroupId,
                sqsRequest.TicketId,
                null,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to CreateSnapshot command
        /// </summary>
        /// <returns>CreateSnapshot</returns>
        public CreateSnapshot ToCommand()
        {
            return new CreateSnapshot(
                new BuildingPersistentLocalId(BuildingPersistentLocalId),
                Provenance);
        }
    }
}
