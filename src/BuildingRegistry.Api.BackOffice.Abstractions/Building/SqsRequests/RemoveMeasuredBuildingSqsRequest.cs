namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class RemoveMeasuredBuildingSqsRequest : SqsRequest
    {
        public RemoveMeasuredBuildingRequest Request { get; set; }
    }
}
