namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class PlanBuildingSqsRequest : SqsRequest, IHasBackOfficeRequest<PlanBuildingBackOfficeRequest>
    {
        public PlanBuildingBackOfficeRequest Request { get; set; }
    }
}
