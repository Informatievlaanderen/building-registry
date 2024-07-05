namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectBuildingUnitRemovalSqsRequest : SqsRequest
    {
        public CorrectBuildingUnitRemovalRequest Request { get; set; }
    }
}
