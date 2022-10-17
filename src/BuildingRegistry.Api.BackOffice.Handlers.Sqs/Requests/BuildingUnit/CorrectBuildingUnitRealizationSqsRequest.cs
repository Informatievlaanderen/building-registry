namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class CorrectBuildingUnitRealizationSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeCorrectBuildingUnitRealizationRequest>
    {
        public BackOfficeCorrectBuildingUnitRealizationRequest Request { get; set; }
    }
}
