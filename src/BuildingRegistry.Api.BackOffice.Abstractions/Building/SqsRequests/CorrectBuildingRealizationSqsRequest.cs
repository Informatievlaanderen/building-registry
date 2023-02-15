namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectBuildingRealizationSqsRequest : SqsRequest
    {
        public CorrectBuildingRealizationRequest Request { get; set; }
    }
}
