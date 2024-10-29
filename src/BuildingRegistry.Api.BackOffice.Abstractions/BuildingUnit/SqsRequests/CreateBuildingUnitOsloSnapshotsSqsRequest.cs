namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public class CreateBuildingUnitOsloSnapshotsSqsRequest : SqsRequest
    {
        public CreateBuildingUnitOsloSnapshotsRequest Request { get; set; }
    }
}
