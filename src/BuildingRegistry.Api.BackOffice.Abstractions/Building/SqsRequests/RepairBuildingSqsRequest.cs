namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RepairBuildingSqsRequest : SqsRequest
    {
        public int BuildingPersistentLocalId { get; set; }
    }
}
