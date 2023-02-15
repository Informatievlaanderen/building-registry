namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class DetachAddressFromBuildingUnitSqsRequest : SqsRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }

        public DetachAddressFromBuildingUnitRequest Request { get; set; }
    }
}
