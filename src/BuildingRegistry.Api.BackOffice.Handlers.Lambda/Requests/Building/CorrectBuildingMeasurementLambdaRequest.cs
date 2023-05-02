namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.Requests;
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using IHasBuildingPersistentLocalId = Abstractions.IHasBuildingPersistentLocalId;

    public sealed record CorrectBuildingMeasurementLambdaRequest : BuildingLambdaRequest, IHasBuildingPersistentLocalId
    {
        public CorrectBuildingMeasurementRequest Request { get; }

        public int BuildingPersistentLocalId { get; }

        public CorrectBuildingMeasurementLambdaRequest(
            string messageGroupId,
            CorrectBuildingMeasurementSqsRequest sqsRequest)
            : base(
                messageGroupId,
                sqsRequest.TicketId,
                ifMatchHeaderValue: null,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            BuildingPersistentLocalId = sqsRequest.BuildingPersistentLocalId;
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to CorrectBuildingMeasurement command
        /// </summary>
        /// <returns>CorrectBuildingMeasurement.</returns>
        public CorrectBuildingMeasurement ToCommand()
            => Request.ToCommand(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
    }
}
