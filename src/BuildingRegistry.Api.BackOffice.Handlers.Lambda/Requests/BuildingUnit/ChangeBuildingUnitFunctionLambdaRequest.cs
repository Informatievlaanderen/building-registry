namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Converters;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.BuildingUnit;

    public sealed record ChangeBuildingUnitFunctionLambdaRequest :
        BuildingUnitLambdaRequest,
        IHasBackOfficeRequest<ChangeBuildingUnitFunctionBackOfficeRequest>
    {
        public ChangeBuildingUnitFunctionBackOfficeRequest Request { get; }

        public int BuildingUnitPersistentLocalId { get; }

        public ChangeBuildingUnitFunctionLambdaRequest(
            string messageGroupId,
            ChangeBuildingUnitFunctionSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.BuildingUnitPersistentLocalId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public ChangeBuildingUnitFunctionLambdaRequest(
            string messageGroupId,
            int buildingUnitPersistentLocalId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            ChangeBuildingUnitFunctionBackOfficeRequest request)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        {
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            Request = request;
        }

        /// <summary>
        /// Map to ChangeBuildingUnitFunction command
        /// </summary>
        /// <returns>ChangeBuildingUnitFunction.</returns>
        public ChangeBuildingUnitFunction ToCommand()
        {
            return new ChangeBuildingUnitFunction(
                BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                Request.Functie.Map(),
                Provenance);
        }
    }
}
