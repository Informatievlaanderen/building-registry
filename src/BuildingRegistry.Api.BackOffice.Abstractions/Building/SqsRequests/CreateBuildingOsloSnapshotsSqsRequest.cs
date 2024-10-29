namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public class CreateBuildingOsloSnapshotsSqsRequest : SqsRequest
    {
        public CreateBuildingOsloSnapshotsRequest Request { get; set; }
    }
}
