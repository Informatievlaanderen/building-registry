namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class NotRealizeBuildingUnitSqsRequest : SqsRequest
    {
        public NotRealizeBuildingUnitRequest Request { get; set; }
    }
}
