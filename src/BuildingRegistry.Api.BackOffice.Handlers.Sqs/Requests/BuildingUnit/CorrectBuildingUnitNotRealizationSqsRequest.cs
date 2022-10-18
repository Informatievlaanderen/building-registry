namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class CorrectBuildingUnitNotRealizationSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeCorrectBuildingUnitNotRealizationRequest>
    {
        public BackOfficeCorrectBuildingUnitNotRealizationRequest Request { get; set; }
    }
}
