namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class RemoveBuildingSqsRequest : SqsRequest
    {
        public RemoveBuildingRequest Request { get; set; }
    }
}
