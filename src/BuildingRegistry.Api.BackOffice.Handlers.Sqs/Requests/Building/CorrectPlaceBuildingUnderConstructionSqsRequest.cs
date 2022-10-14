namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;

    public sealed class CorrectPlaceBuildingUnderConstructionSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeCorrectPlaceBuildingUnderConstructionRequest>
    {
        public BackOfficeCorrectPlaceBuildingUnderConstructionRequest Request { get; set; }
    }
}
