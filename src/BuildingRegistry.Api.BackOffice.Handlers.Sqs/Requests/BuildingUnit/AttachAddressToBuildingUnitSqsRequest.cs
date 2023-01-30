namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class AttachAddressToBuildingUnitSqsRequest : SqsRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }

        public AttachAddressToBuildingUnitRequest Request { get; set; }
    }
}
