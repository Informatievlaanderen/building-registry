namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectPlaceBuildingUnderConstructionSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectPlaceBuildingUnderConstructionBackOfficeRequest>
    {
        public CorrectPlaceBuildingUnderConstructionBackOfficeRequest Request { get; set; }
    }
}
