namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ChangeBuildingOutlineSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeBuildingOutlineBackOfficeRequest>
    {
        public int BuildingPersistentLocalId { get; set; }

        public ChangeBuildingOutlineBackOfficeRequest Request { get; set; }
    }
}
