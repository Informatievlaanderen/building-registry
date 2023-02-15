namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectBuildingUnitPositionSqsRequest : SqsRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }

        public CorrectBuildingUnitPositionRequest Request { get; set; }
    }
}
