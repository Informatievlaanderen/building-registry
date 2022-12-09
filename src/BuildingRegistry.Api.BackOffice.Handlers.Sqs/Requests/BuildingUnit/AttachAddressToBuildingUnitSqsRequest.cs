namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class AttachAddressToBuildingUnitSqsRequest : SqsRequest, IHasBackOfficeRequest<AttachAddressToBuildingUnitBackOfficeRequest>
    {
        public int BuildingUnitPersistentLocalId { get; set; }

        public AttachAddressToBuildingUnitBackOfficeRequest Request { get; set; }
    }
}
