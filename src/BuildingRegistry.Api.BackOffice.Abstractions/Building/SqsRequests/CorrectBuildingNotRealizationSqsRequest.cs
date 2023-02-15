namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectBuildingNotRealizationSqsRequest : SqsRequest
    {
        public CorrectBuildingNotRealizationRequest Request { get; set; }
    }
}
