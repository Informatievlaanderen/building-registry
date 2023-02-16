namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record PlaceBuildingUnderConstructionLambdaRequest : BuildingLambdaRequest, Abstractions.IHasBuildingPersistentLocalId
    {
        public PlaceBuildingUnderConstructionRequest Request { get; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public PlaceBuildingUnderConstructionLambdaRequest(
            string messageGroupId,
            PlaceBuildingUnderConstructionSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
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
