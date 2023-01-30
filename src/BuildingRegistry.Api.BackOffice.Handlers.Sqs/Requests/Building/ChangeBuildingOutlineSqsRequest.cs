namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ChangeBuildingOutlineSqsRequest : SqsRequest
    {
        public int BuildingPersistentLocalId { get; set; }

        public ChangeBuildingOutlineRequest Request { get; set; }
    }
}
