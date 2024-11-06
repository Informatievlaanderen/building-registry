namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CreateBuildingSnapshotSqsRequest : SqsRequest
    {
        public CreateBuildingSnapshotRequest Request { get; set; }
    }
}
