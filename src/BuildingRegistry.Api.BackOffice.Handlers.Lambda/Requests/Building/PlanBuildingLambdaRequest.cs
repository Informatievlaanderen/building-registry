namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.Building;

    public sealed record PlanBuildingLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<PlanBuildingBackOfficeRequest>
    {
        public PlanBuildingBackOfficeRequest Request { get; }

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
            PlanBuildingBackOfficeRequest request)
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
