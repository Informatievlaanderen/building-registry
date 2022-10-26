namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;

    public sealed class RealizeBuildingSqsRequest : SqsRequest, IHasBackOfficeRequest<RealizeBuildingBackOfficeRequest>
    {
        public RealizeBuildingBackOfficeRequest Request { get; set; }
    }
}
