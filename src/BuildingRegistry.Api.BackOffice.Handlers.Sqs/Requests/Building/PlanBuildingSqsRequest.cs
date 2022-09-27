namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;

    public sealed class PlanBuildingSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficePlanBuildingRequest>
    {
        public BackOfficePlanBuildingRequest Request { get; set; }
    }
}
