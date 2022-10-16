namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class RealizeBuildingUnitSqsRequest : SqsRequest, IHasBackOfficeRequest<RealizeBuildingUnitBackOfficeRequest>
    {
        public RealizeBuildingUnitBackOfficeRequest Request { get; set; }
    }
}
