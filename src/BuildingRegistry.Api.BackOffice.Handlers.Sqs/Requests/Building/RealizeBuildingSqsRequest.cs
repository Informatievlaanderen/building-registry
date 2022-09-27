namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;

    public sealed class RealizeBuildingSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeRealizeBuildingRequest>
    {
        public BackOfficeRealizeBuildingRequest Request { get; set; }
    }
}
