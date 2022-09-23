namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;

    public sealed class SqsRealizeBuildingRequest : SqsRequest, IHasBackOfficeRequest<BuildingBackOfficeRealizeRequest>
    {
        public BuildingBackOfficeRealizeRequest Request { get; set; }
    }
}
