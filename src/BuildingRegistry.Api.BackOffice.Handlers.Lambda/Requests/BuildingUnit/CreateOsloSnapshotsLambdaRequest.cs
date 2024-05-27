namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using BuildingRegistry.AllStream.Commands;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Building;

    public sealed record CreateOsloSnapshotsLambdaRequest : SqsLambdaRequest
    {
        public CreateOsloSnapshotsRequest Request { get; }


        public CreateOsloSnapshotsLambdaRequest(
            string messageGroupId,
            CreateOsloSnapshotsSqsRequest sqsRequest)
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
                Request.BuildingUnitPersistentLocalIds.Select(x => new BuildingUnitPersistentLocalId(x)),
                Provenance);
        }
    }
}
