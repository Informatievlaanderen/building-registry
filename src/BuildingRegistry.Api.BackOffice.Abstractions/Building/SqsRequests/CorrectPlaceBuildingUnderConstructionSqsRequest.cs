namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CorrectPlaceBuildingUnderConstructionSqsRequest : SqsRequest
    {
        public CorrectPlaceBuildingUnderConstructionRequest Request { get; set; }
    }
}
