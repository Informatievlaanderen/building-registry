namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class ChangeBuildingMeasurementSqsRequest : SqsRequest
    {
        public int BuildingPersistentLocalId { get; set; }

        public ChangeBuildingMeasurementRequest Request { get; set; }
    }
}
