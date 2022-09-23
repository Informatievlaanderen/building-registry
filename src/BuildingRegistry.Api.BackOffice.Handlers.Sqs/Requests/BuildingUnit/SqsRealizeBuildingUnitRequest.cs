namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class SqsRealizeBuildingUnitRequest : SqsRequest, IHasBackOfficeRequest<BuildingUnitBackOfficeRealizeRequest>
    {
        public BuildingUnitBackOfficeRealizeRequest Request { get; set; }
    }
}
