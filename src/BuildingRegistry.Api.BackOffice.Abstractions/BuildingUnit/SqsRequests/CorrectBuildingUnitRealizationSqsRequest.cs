namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectBuildingUnitRealizationSqsRequest : SqsRequest
    {
        public CorrectBuildingUnitRealizationRequest Request { get; set; }
    }
}
