namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class PlanBuildingUnitSqsRequest : SqsRequest
    {
        public PlanBuildingUnitRequest Request { get; set; }
    }
}
