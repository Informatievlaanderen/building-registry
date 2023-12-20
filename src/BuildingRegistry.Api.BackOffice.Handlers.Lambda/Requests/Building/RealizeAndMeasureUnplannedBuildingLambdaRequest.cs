namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building;
    using Abstractions.Building.Requests;
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record RealizeAndMeasureUnplannedBuildingLambdaRequest : BuildingLambdaRequest, IHasBuildingPersistentLocalId
    {
        public int BuildingPersistentLocalId { get; }

        public RealizeAndMeasureUnplannedBuildingRequest Request { get; }

        public RealizeAndMeasureUnplannedBuildingLambdaRequest(string messageGroupId, RealizeAndMeasureUnplannedBuildingSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, null, sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
            BuildingPersistentLocalId = new BuildingPersistentLocalId(sqsRequest.BuildingPersistentLocalId);
        }

        /// <summary>
        /// Map to command
        /// </summary>
        /// <returns>RealizeAndMeasureUnplannedBuilding.</returns>
        public RealizeAndMeasureUnplannedBuilding ToCommand()
        {
           return new RealizeAndMeasureUnplannedBuilding(
                new BuildingPersistentLocalId(BuildingPersistentLocalId),
                Request.GrbData.GeometriePolygoon.ToExtendedWkbGeometry(),
                Request.GrbData.ToBuildingGrbData(),
                CommandProvenance);
        }
    }
}
