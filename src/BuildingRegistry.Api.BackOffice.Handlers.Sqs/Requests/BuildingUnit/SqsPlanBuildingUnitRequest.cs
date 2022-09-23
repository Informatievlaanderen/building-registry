namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using BuildingRegistry.Api.BackOffice.Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class SqsPlanBuildingUnitRequest : SqsRequest, IHasBackOfficeRequest<BuildingUnitBackOfficePlanRequest>
    {
        public BuildingUnitBackOfficePlanRequest Request { get; set; }
    }
}
