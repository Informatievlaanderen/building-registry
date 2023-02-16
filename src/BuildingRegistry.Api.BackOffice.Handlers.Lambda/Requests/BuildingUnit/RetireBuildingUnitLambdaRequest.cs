namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions.BuildingUnit.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using IHasBuildingUnitPersistentLocalId = Abstractions.IHasBuildingUnitPersistentLocalId;

    public sealed record RetireBuildingUnitLambdaRequest : BuildingUnitLambdaRequest, IHasBuildingUnitPersistentLocalId
    {
        public RetireBuildingUnitRequest Request { get; }

        public int BuildingUnitPersistentLocalId => Request.BuildingUnitPersistentLocalId;

        public RetireBuildingUnitLambdaRequest(
            string messageGroupId,
            RetireBuildingUnitSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to RetireBuildingUnit command
        /// </summary>
        /// <returns>RetireBuildingUnit.</returns>
        public RetireBuildingUnit ToCommand()
        {
            return new RetireBuildingUnit(BuildingPersistentLocalId, new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId), Provenance);
        }
    }
}
