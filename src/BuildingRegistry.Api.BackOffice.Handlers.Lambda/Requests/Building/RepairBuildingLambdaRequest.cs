namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using IHasBuildingPersistentLocalId = Abstractions.IHasBuildingPersistentLocalId;

    public sealed record RepairBuildingLambdaRequest : BuildingLambdaRequest, IHasBuildingPersistentLocalId
    {
        public int BuildingPersistentLocalId { get; }

        public RepairBuildingLambdaRequest(string messageGroupId, RepairBuildingSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            BuildingPersistentLocalId = sqsRequest.BuildingPersistentLocalId;
        }

        /// <summary>
        /// Map to RepairBuilding command.
        /// </summary>
        /// <returns>RepairBuilding.</returns>
        public RepairBuilding ToCommand()
        {
            return new RepairBuilding(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
