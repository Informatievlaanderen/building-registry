namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions.BuildingUnit.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using IHasBuildingUnitPersistentLocalId = Abstractions.IHasBuildingUnitPersistentLocalId;

    public sealed record DeregulateBuildingUnitLambdaRequest : BuildingUnitLambdaRequest, IHasBuildingUnitPersistentLocalId
    {
        public DeregulateBuildingUnitRequest Request { get; }

        public int BuildingUnitPersistentLocalId => Request.BuildingUnitPersistentLocalId;

        public DeregulateBuildingUnitLambdaRequest(
            string messageGroupId,
            DeregulateBuildingUnitSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to DeregulateBuildingUnit command
        /// </summary>
        /// <returns>DeregulateBuildingUnit.</returns>
        public DeregulateBuildingUnit ToCommand()
        {
            return new DeregulateBuildingUnit(BuildingPersistentLocalId, new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId), Provenance);
        }
    }
}
