namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions.BuildingUnit.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using IHasBuildingUnitPersistentLocalId = Abstractions.IHasBuildingUnitPersistentLocalId;

    public sealed record CorrectBuildingUnitRealizationLambdaRequest : BuildingUnitLambdaRequest, IHasBuildingUnitPersistentLocalId
    {
        public CorrectBuildingUnitRealizationRequest Request { get; }

        public int BuildingUnitPersistentLocalId => Request.BuildingUnitPersistentLocalId;

        public CorrectBuildingUnitRealizationLambdaRequest(
            string messageGroupId,
            CorrectBuildingUnitRealizationSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to CorrectBuildingUnitRealization command
        /// </summary>
        /// <returns>CorrectBuildingUnitRealization.</returns>
        public CorrectBuildingUnitRealization ToCommand()
        {
            return new CorrectBuildingUnitRealization(BuildingPersistentLocalId, new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId), Provenance);
        }
    }
}
