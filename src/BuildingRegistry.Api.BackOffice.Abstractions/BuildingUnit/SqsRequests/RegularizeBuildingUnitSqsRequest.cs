namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class RegularizeBuildingUnitSqsRequest : SqsRequest
    {
        public RegularizeBuildingUnitRequest Request { get; set; }
    }
}
