namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectBuildingUnitNotRealizationSqsRequest : SqsRequest
    {
        public CorrectBuildingUnitNotRealizationRequest Request { get; set; }
    }
}
