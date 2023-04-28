namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class MeasureBuildingSqsRequest : SqsRequest
    {
        public int BuildingPersistentLocalId { get; set; }

        public MeasureBuildingRequest Request { get; set; }
    }
}
