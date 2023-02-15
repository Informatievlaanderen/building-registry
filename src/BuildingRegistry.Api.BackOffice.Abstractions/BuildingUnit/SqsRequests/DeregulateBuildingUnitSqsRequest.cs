namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class DeregulateBuildingUnitSqsRequest : SqsRequest
    {
        public DeregulateBuildingUnitRequest Request { get; set; }
    }
}
