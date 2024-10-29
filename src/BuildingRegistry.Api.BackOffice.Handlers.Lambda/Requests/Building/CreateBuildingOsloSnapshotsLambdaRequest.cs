namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using BuildingRegistry.AllStream.Commands;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Building;

    public sealed record CreateBuildingOsloSnapshotsLambdaRequest : SqsLambdaRequest
    {
        public CreateBuildingOsloSnapshotsRequest Request { get; }

        public CreateBuildingOsloSnapshotsLambdaRequest(
            string messageGroupId,
            CreateBuildingOsloSnapshotsSqsRequest sqsRequest)
            : base(
                messageGroupId,
                sqsRequest.TicketId,
                null,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to CreateOsloSnapshots command
        /// </summary>
        /// <returns>CreateOsloSnapshots</returns>
        public CreateOsloSnapshots ToCommand()
        {
            return new CreateOsloSnapshots(
                Request.BuildingPersistentLocalIds.Select(x => new BuildingPersistentLocalId(x)),
                [],
                Provenance);
        }
    }
}
