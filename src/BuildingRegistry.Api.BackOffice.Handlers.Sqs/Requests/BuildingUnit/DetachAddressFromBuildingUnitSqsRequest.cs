namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class DetachAddressFromBuildingUnitSqsRequest : SqsRequest, IHasBackOfficeRequest<DetachAddressFromBuildingUnitBackOfficeRequest>
    {
        public int BuildingUnitPersistentLocalId { get; set; }

        public DetachAddressFromBuildingUnitBackOfficeRequest Request { get; set; }
    }
}
