namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class DeregulateBuildingUnitSqsRequest : SqsRequest
    {
        public DeregulateBuildingUnitRequest Request { get; set; }
    }
}
