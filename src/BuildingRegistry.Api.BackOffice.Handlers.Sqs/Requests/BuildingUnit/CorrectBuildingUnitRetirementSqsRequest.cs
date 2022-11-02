namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;

    public sealed class CorrectBuildingUnitRetirementSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectBuildingUnitRetirementBackOfficeRequest>
    {
        public CorrectBuildingUnitRetirementBackOfficeRequest Request { get; set; }
    }
}
