namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class CorrectBuildingUnitRealizationSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectBuildingUnitRealizationBackOfficeRequest>
    {
        public CorrectBuildingUnitRealizationBackOfficeRequest Request { get; set; }
    }
}
