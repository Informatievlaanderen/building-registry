namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Building;
    using Requests;

    public sealed class DemolishBuildingSqsRequest : SqsRequest
    {
        public int BuildingPersistentLocalId { get; set; }
        public DemolishBuildingRequest Request { get; set; }
    }
}
