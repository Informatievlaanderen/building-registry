namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.Requests;
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record RealizeAndMeasureUnplannedBuildingLambdaRequest : BuildingLambdaRequest
    {
        public BuildingPersistentLocalId BuildingPersistentLocalId { get; }

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
        public RealizeAndMeasureUnplannedBuilding ToCommand() =>  Request.ToCommand(BuildingPersistentLocalId, Provenance);
    }
}
