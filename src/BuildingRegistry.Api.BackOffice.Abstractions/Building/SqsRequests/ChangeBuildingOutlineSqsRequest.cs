namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class ChangeBuildingOutlineSqsRequest : SqsRequest
    {
        public int BuildingPersistentLocalId { get; set; }

        public ChangeBuildingOutlineRequest Request { get; set; }
    }
}
