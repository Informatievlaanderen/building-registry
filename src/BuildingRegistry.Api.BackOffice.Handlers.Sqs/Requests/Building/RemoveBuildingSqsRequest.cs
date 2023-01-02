namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RemoveBuildingSqsRequest : SqsRequest, IHasBackOfficeRequest<RemoveBuildingRequest>
    {
        public RemoveBuildingRequest Request { get; set; }
    }
}
