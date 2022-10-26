namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class CorrectBuildingUnitNotRealizationSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectBuildingUnitNotRealizationBackOfficeRequest>
    {
        public CorrectBuildingUnitNotRealizationBackOfficeRequest Request { get; set; }
    }
}
