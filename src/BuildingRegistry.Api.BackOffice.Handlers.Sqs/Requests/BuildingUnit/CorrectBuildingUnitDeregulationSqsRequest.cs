namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class CorrectBuildingUnitDeregulationSqsRequest : SqsRequest
    {
        public CorrectBuildingUnitDeregulationRequest Request { get; set; }
    }
}
