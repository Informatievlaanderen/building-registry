namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class PlanBuildingUnitSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficePlanBuildingUnitRequest>
    {
        public BackOfficePlanBuildingUnitRequest Request { get; set; }
    }
}
