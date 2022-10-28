namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.BuildingUnit;

    public sealed record RetireBuildingUnitLambdaRequest :
        BuildingUnitLambdaRequest,
        IHasBackOfficeRequest<RetireBuildingUnitBackOfficeRequest>,
        IHasBuildingUnitPersistentLocalId
    {
        public RetireBuildingUnitBackOfficeRequest Request { get; }

        public int BuildingUnitPersistentLocalId => Request.BuildingUnitPersistentLocalId;

        public RetireBuildingUnitLambdaRequest(
            string messageGroupId,
            RetireBuildingUnitSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public RetireBuildingUnitLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            RetireBuildingUnitBackOfficeRequest request)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        /// <summary>
        /// Map to RetireBuildingUnit command
        /// </summary>
        /// <returns>RetireBuildingUnit.</returns>
        public RetireBuildingUnit ToCommand()
        {
            return new RetireBuildingUnit(BuildingPersistentLocalId, new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId), Provenance);
        }
    }
}
