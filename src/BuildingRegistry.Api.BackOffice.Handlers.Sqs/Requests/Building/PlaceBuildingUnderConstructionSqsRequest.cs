namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class PlaceBuildingUnderConstructionSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficePlaceBuildingUnderConstructionRequest>
    {
        public BackOfficePlaceBuildingUnderConstructionRequest Request { get; set; }
    }
}
