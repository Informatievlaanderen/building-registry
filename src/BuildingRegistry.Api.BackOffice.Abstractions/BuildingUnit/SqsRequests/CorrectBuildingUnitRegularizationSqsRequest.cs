namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectBuildingUnitRegularizationSqsRequest : SqsRequest
    {
        public CorrectBuildingUnitRegularizationRequest Request { get; set; }
    }
}
