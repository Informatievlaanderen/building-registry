namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record CorrectPlaceBuildingUnderConstructionLambdaRequest : BuildingLambdaRequest, Abstractions.IHasBuildingPersistentLocalId
    {
        public CorrectPlaceBuildingUnderConstructionRequest Request { get; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public CorrectPlaceBuildingUnderConstructionLambdaRequest(
            string messageGroupId,
            CorrectPlaceBuildingUnderConstructionSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to CorrectBuildingPlaceUnderConstruction command.
        /// </summary>
        /// <returns>CorrectBuildingPlaceUnderConstruction.</returns>
        public CorrectBuildingPlaceUnderConstruction ToCommand()
        {
            return new CorrectBuildingPlaceUnderConstruction(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
