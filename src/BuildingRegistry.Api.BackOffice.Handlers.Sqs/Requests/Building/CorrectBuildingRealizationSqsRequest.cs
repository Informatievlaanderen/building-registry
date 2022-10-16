namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class CorrectBuildingRealizationSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeCorrectBuildingRealizationRequest>
    {
        public BackOfficeCorrectBuildingRealizationRequest Request { get; set; }
    }
}
