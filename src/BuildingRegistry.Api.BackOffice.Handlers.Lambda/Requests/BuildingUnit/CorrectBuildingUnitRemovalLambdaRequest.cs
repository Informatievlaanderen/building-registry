namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions.BuildingUnit.Requests;
    using Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using IHasBuildingUnitPersistentLocalId = Abstractions.IHasBuildingUnitPersistentLocalId;

    public sealed record CorrectBuildingUnitRemovalLambdaRequest : BuildingUnitLambdaRequest, IHasBuildingUnitPersistentLocalId
    {
        public CorrectBuildingUnitRemovalRequest Request { get; }

        public int BuildingUnitPersistentLocalId => Request.BuildingUnitPersistentLocalId;

        public CorrectBuildingUnitRemovalLambdaRequest(
            string messageGroupId,
            CorrectBuildingUnitRemovalSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to CorrectBuildingUnitRemoval command
        /// </summary>
        /// <returns>CorrectBuildingUnitRemoval.</returns>
        public CorrectBuildingUnitRemoval ToCommand()
        {
            return new CorrectBuildingUnitRemoval(
                BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                Provenance);
        }
    }
}
