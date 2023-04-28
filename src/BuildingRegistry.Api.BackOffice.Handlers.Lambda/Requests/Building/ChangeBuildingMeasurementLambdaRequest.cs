namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Building.Commands;

    public sealed record ChangeBuildingMeasurementLambdaRequest : BuildingLambdaRequest, IHasBuildingPersistentLocalId
    {
        public ChangeBuildingMeasurementRequest Request { get; }

        public int BuildingPersistentLocalId { get; }

        public ChangeBuildingMeasurementLambdaRequest(
            string messageGroupId,
            ChangeBuildingMeasurementSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            BuildingPersistentLocalId = sqsRequest.BuildingPersistentLocalId;
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to ChangeBuildingMeasurement command
        /// </summary>
        /// <returns>ChangeBuildingMeasurement.</returns>
        public ChangeBuildingMeasurement ToCommand() => Request.ToCommand(BuildingPersistentLocalId, Provenance);
    }
}
