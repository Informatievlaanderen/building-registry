namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Converters;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.BuildingUnit;

    public sealed record CorrectBuildingUnitPositionLambdaRequest :
        BuildingUnitLambdaRequest,
        IHasBackOfficeRequest<CorrectBuildingUnitPositionBackOfficeRequest>
    {
        public CorrectBuildingUnitPositionBackOfficeRequest Request { get; }

        public int BuildingUnitPersistentLocalId { get; }

        public CorrectBuildingUnitPositionLambdaRequest(
            string messageGroupId,
            CorrectBuildingUnitPositionSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.BuildingUnitPersistentLocalId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public CorrectBuildingUnitPositionLambdaRequest(
            string messageGroupId,
            int buildingUnitPersistentLocalId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            CorrectBuildingUnitPositionBackOfficeRequest request)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        {
            BuildingUnitPersistentLocalId = buildingUnitPersistentLocalId;
            Request = request;
        }

        /// <summary>
        /// Map to CorrectBuildingUnitPosition command
        /// </summary>
        /// <returns>CorrectBuildingUnitPosition.</returns>
        public CorrectBuildingUnitPosition ToCommand()
        {
            return new CorrectBuildingUnitPosition(
                BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                Request.PositieGeometrieMethode.Map(),
                string.IsNullOrWhiteSpace(Request.Positie) ? null : Request.Positie.ToExtendedWkbGeometry(),
                Provenance);
        }
    }
}