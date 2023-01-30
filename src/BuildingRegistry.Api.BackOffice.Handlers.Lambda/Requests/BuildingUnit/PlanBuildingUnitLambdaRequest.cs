namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Converters;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.BuildingUnit;

    public sealed record PlanBuildingUnitLambdaRequest : BuildingUnitLambdaRequest
    {
        public PlanBuildingUnitRequest Request { get; }

        public PlanBuildingUnitLambdaRequest(string messageGroupId, PlanBuildingUnitSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.TicketId,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public PlanBuildingUnitLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            PlanBuildingUnitRequest request)
            : base(messageGroupId, ticketId, null, provenance, metadata)
        {
            Request = request;
        }

        /// <summary>
        /// Map to PlanBuildingUnit command
        /// </summary>
        /// <returns>PlanBuildingUnit.</returns>
        public PlanBuildingUnit ToCommand(
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            return new PlanBuildingUnit(
                BuildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                Request.PositieGeometrieMethode.Map(),
                string.IsNullOrWhiteSpace(Request.Positie) ? null : Request.Positie.ToExtendedWkbGeometry(),
                Request.Functie.Map(),
                Request.AfwijkingVastgesteld,
                Provenance);
        }
    }
}
