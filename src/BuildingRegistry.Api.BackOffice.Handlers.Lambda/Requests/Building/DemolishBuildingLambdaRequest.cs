namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.Requests;
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using IHasBuildingPersistentLocalId = Abstractions.IHasBuildingPersistentLocalId;

    public sealed record DemolishBuildingLambdaRequest : BuildingLambdaRequest, IHasBuildingPersistentLocalId
    {
        public DemolishBuildingRequest Request { get; }

        public int BuildingPersistentLocalId { get; }

        public DemolishBuildingLambdaRequest(
            string messageGroupId,
            DemolishBuildingSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
            BuildingPersistentLocalId = sqsRequest.BuildingPersistentLocalId;
        }

        /// <summary>
        /// Map to DemolishBuilding command
        /// </summary>
        /// <returns>DemolishBuilding.</returns>
        public DemolishBuilding ToCommand()
        {
            return new DemolishBuilding(new BuildingPersistentLocalId(BuildingPersistentLocalId),
                Request.GrbData.ToBuildingGrbData(),
                Provenance);
        }
    }
}
