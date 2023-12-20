namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record RealizeBuildingLambdaRequest : BuildingLambdaRequest, Abstractions.IHasBuildingPersistentLocalId
    {
        public RealizeBuildingRequest Request { get; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public RealizeBuildingLambdaRequest(
            string messageGroupId,
            RealizeBuildingSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to RealizeBuilding command
        /// </summary>
        /// <returns>RealizeBuilding.</returns>
        public RealizeBuilding ToCommand()
        {
            return new RealizeBuilding(new BuildingPersistentLocalId(BuildingPersistentLocalId), CommandProvenance);
        }
    }
}
