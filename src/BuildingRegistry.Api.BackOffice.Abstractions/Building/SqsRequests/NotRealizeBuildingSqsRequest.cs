namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class NotRealizeBuildingSqsRequest : SqsRequest
    {
        public NotRealizeBuildingRequest Request { get; set; }
    }
}
