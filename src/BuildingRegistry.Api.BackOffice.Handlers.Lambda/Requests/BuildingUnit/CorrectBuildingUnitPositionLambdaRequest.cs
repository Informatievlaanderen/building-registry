namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions.Building;
    using Abstractions.BuildingUnit.Converters;
    using Abstractions.BuildingUnit.Requests;
    using Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record CorrectBuildingUnitPositionLambdaRequest : BuildingUnitLambdaRequest
    {
        public CorrectBuildingUnitPositionRequest Request { get; }

        public int BuildingUnitPersistentLocalId { get; }

        public CorrectBuildingUnitPositionLambdaRequest(
            string messageGroupId,
            CorrectBuildingUnitPositionSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            BuildingUnitPersistentLocalId = sqsRequest.BuildingUnitPersistentLocalId;
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to CorrectBuildingUnitPosition command
        /// </summary>
        /// <returns>CorrectBuildingUnitPosition.</returns>
        public CorrectBuildingUnitPosition ToCommand()
        {
            return new CorrectBuildingUnitPosition(
                BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                Request.PositieGeometrieMethode.Map(),
                string.IsNullOrWhiteSpace(Request.Positie) ? null : Request.Positie.ToExtendedWkbGeometry(),
                Provenance);
        }
    }
}
