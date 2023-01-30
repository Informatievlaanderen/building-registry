namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectPlaceBuildingUnderConstructionSqsRequest : SqsRequest
    {
        public CorrectPlaceBuildingUnderConstructionRequest Request { get; set; }
    }
}
