namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record PlanBuildingLambdaRequest : BuildingLambdaRequest
    {
        public PlanBuildingRequest Request { get; }

        public PlanBuildingLambdaRequest(string messageGroupId, PlanBuildingSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.TicketId,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public PlanBuildingLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            PlanBuildingRequest request)
            : base(messageGroupId, ticketId, null, provenance, metadata)
        {
            Request = request;
        }

        /// <summary>
        /// Map to PlanBuilding command
        /// </summary>
        /// <returns>PlanBuilding.</returns>
        public PlanBuilding ToCommand(BuildingPersistentLocalId buildingPersistentLocalId)
        {
            return new PlanBuilding(
                buildingPersistentLocalId,
                Request.GeometriePolygoon.ToExtendedWkbGeometry(),
                Provenance);
        }
    }
}
