namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;

    public sealed class SqsPlanBuildingRequest : SqsRequest, IHasBackOfficeRequest<BuildingBackOfficePlanRequest>
    {
        public BuildingBackOfficePlanRequest Request { get; set; }
    }
}
