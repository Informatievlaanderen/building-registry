namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectBuildingRealizationSqsRequest : SqsRequest
    {
        public CorrectBuildingRealizationRequest Request { get; set; }
    }
}
