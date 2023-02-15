namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class AttachAddressToBuildingUnitSqsRequest : SqsRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }

        public AttachAddressToBuildingUnitRequest Request { get; set; }
    }
}
