namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;

    public sealed class SqsPlaceBuildingUnderConstructionRequest : SqsRequest, IHasBackOfficeRequest<BuildingBackOfficePlaceUnderConstructionRequest>
    {
        public BuildingBackOfficePlaceUnderConstructionRequest Request { get; set; }
    }
}
