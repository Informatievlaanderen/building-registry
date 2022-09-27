namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;

    public sealed class NotRealizeBuildingSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeNotRealizeBuildingRequest>
    {
        public BackOfficeNotRealizeBuildingRequest Request { get; set; }
    }
}
