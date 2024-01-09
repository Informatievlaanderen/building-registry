namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record PlanBuildingLambdaRequest : BuildingLambdaRequest
    {
        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

        public PlanBuildingRequest Request { get; }

        public PlanBuildingLambdaRequest(string messageGroupId, PlanBuildingSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, null, sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
            BuildingPersistentLocalId = new BuildingPersistentLocalId(sqsRequest.BuildingPersistentLocalId);
        }

        /// <summary>
        /// Map to PlanBuilding command
        /// </summary>
        /// <returns>PlanBuilding.</returns>
        public PlanBuilding ToCommand()
        {
            return new PlanBuilding(
                BuildingPersistentLocalId,
                Request.GeometriePolygoon.ToExtendedWkbGeometry(),
                Provenance);
        }
    }
}
