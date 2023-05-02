namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectBuildingMeasurementSqsRequest : SqsRequest
    {
        public int BuildingPersistentLocalId { get; set; }

        public CorrectBuildingMeasurementRequest Request { get; set; }
    }
}
