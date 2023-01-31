namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class ChangeBuildingUnitFunctionSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeBuildingUnitFunctionBackOfficeRequest>
    {
        public int BuildingUnitPersistentLocalId { get; set; }

        public ChangeBuildingUnitFunctionBackOfficeRequest Request { get; set; }
    }
}
