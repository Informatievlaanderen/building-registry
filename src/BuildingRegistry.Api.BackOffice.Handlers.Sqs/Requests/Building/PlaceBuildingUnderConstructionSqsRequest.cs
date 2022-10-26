namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;

    public sealed class PlaceBuildingUnderConstructionSqsRequest : SqsRequest, IHasBackOfficeRequest<PlaceBuildingUnderConstructionBackOfficeRequest>
    {
        public PlaceBuildingUnderConstructionBackOfficeRequest Request { get; set; }
    }
}
