namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectBuildingUnitDeregulationSqsRequest : SqsRequest
    {
        public CorrectBuildingUnitDeregulationRequest Request { get; set; }
    }
}
