namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public class CreateOsloSnapshotsSqsRequest : SqsRequest
    {
        public CreateOsloSnapshotsRequest Request { get; set; }
    }
}
