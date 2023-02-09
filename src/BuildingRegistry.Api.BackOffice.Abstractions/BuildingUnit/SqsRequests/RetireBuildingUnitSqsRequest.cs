namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class RetireBuildingUnitSqsRequest : SqsRequest
    {
        public RetireBuildingUnitRequest Request { get; set; }
    }
}