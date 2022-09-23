namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;

    public sealed class SqsNotRealizeBuildingRequest : SqsRequest, IHasBackOfficeRequest<BuildingBackOfficeNotRealizeRequest>
    {
        public BuildingBackOfficeNotRealizeRequest Request { get; set; }
    }
}
