namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class RealizeBuildingSqsRequest : SqsRequest
    {
        public RealizeBuildingRequest Request { get; set; }
    }
}
