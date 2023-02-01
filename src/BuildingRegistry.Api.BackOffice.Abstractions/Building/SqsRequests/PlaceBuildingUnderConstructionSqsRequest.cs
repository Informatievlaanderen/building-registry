namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;

    public sealed class PlaceBuildingUnderConstructionSqsRequest : SqsRequest
    {
        public PlaceBuildingUnderConstructionRequest Request { get; set; }
    }
}
