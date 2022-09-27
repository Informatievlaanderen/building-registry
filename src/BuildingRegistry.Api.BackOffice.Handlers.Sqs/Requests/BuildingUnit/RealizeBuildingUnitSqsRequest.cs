namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class RealizeBuildingUnitSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeRealizeBuildingUnitRequest>
    {
        public BackOfficeRealizeBuildingUnitRequest Request { get; set; }
    }
}
