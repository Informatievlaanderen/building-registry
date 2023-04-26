namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record DemolishBuildingLambdaRequest : BuildingLambdaRequest, Abstractions.IHasBuildingPersistentLocalId
    {
        public DemolishBuildingRequest Request { get; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public DemolishBuildingLambdaRequest(
            string messageGroupId,
            DemolishBuildingSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to DemolishBuilding command
        /// </summary>
        /// <returns>DemolishBuilding.</returns>
        public DemolishBuilding ToCommand()
        {
            return new DemolishBuilding(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
