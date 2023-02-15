namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class RetireBuildingUnitSqsRequest : SqsRequest
    {
        public RetireBuildingUnitRequest Request { get; set; }
    }
}
