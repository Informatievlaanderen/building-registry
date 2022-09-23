namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class SqsNotRealizeBuildingUnitRequest : SqsRequest, IHasBackOfficeRequest<BuildingUnitBackOfficeNotRealizeRequest>
    {
        public BuildingUnitBackOfficeNotRealizeRequest Request { get; set; }
    }
}
