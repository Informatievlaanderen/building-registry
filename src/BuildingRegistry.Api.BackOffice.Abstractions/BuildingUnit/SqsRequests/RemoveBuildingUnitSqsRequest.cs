namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class RemoveBuildingUnitSqsRequest : SqsRequest
    {
        public RemoveBuildingUnitRequest Request { get; set; }
    }
}
