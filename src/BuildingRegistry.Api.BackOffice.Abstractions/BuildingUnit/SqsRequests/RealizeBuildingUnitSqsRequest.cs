namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class RealizeBuildingUnitSqsRequest : SqsRequest
    {
        public RealizeBuildingUnitRequest Request { get; set; }
    }
}
