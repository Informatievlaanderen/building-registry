namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions.Building;
    using Abstractions.BuildingUnit.Converters;
    using Abstractions.BuildingUnit.Requests;
    using Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record PlanBuildingUnitLambdaRequest : BuildingUnitLambdaRequest
    {
        public BuildingUnitPersistentLocalId BuildingUnitPersistentLocalId { get; }

        public PlanBuildingUnitRequest Request { get; }

        public PlanBuildingUnitLambdaRequest(string messageGroupId, PlanBuildingUnitSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, null, sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
            BuildingUnitPersistentLocalId = new BuildingUnitPersistentLocalId(sqsRequest.BuildingUnitPersistentLocalId);
        }

        /// <summary>
        /// Map to PlanBuildingUnit command
        /// </summary>
        /// <returns>PlanBuildingUnit.</returns>
        public PlanBuildingUnit ToCommand()
        {
            return new PlanBuildingUnit(
                BuildingPersistentLocalId,
                BuildingUnitPersistentLocalId,
                Request.PositieGeometrieMethode.Map(),
                string.IsNullOrWhiteSpace(Request.Positie) ? null : Request.Positie.ToExtendedWkbGeometry(),
                Request.Functie.Map(),
                Request.AfwijkingVastgesteld,
                Provenance);
        }
    }
}
