namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class NotRealizeBuildingUnitSqsRequest : SqsRequest
    {
        public NotRealizeBuildingUnitRequest Request { get; set; }
    }
}
