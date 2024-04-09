namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class MoveBuildingUnitSqsRequest : SqsRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }

        public MoveBuildingUnitRequest Request { get; set; }
    }
}
