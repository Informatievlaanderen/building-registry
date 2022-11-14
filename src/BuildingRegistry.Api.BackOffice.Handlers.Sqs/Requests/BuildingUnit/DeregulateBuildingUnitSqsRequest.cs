namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class DeregulateBuildingUnitSqsRequest : SqsRequest, IHasBackOfficeRequest<DeregulateBuildingUnitBackOfficeRequest>
    {
        public DeregulateBuildingUnitBackOfficeRequest Request { get; set; }
    }
}
