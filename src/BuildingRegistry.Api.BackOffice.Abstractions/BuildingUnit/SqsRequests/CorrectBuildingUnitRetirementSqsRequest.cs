namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectBuildingUnitRetirementSqsRequest : SqsRequest
    {
        public CorrectBuildingUnitRetirementRequest Request { get; set; }
    }
}
