namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions.BuildingUnit.Requests;
    using Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using IHasBuildingUnitPersistentLocalId = Abstractions.IHasBuildingUnitPersistentLocalId;

    public sealed record CorrectBuildingUnitDeregulationLambdaRequest :
        BuildingUnitLambdaRequest,
        IHasBuildingUnitPersistentLocalId
    {
        public CorrectBuildingUnitDeregulationRequest Request { get; }

        public int BuildingUnitPersistentLocalId => Request.BuildingUnitPersistentLocalId;

        public CorrectBuildingUnitDeregulationLambdaRequest(
            string messageGroupId,
            CorrectBuildingUnitDeregulationSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue, sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to CorrectBuildingUnitDeregulation command
        /// </summary>
        /// <returns>CorrectBuildingUnitDeregulation.</returns>
        public CorrectBuildingUnitDeregulation ToCommand()
        {
            return new CorrectBuildingUnitDeregulation(
                BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                Provenance);
        }
    }
}
