namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.BuildingUnit;

    public sealed record CorrectBuildingUnitRetirementLambdaRequest :
        BuildingUnitLambdaRequest,
        IHasBackOfficeRequest<CorrectBuildingUnitRetirementBackOfficeRequest>,
        IHasBuildingUnitPersistentLocalId
    {
        public CorrectBuildingUnitRetirementBackOfficeRequest Request { get; }

        public int BuildingUnitPersistentLocalId => Request.BuildingUnitPersistentLocalId;

        public CorrectBuildingUnitRetirementLambdaRequest(
            string messageGroupId,
            CorrectBuildingUnitRetirementSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public CorrectBuildingUnitRetirementLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            CorrectBuildingUnitRetirementBackOfficeRequest request)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        /// <summary>
        /// Map to CorrectBuildingUnitRetirement command
        /// </summary>
        /// <returns>CorrectBuildingUnitRetirement.</returns>
        public CorrectBuildingUnitRetirement ToCommand()
        {
            return new CorrectBuildingUnitRetirement(BuildingPersistentLocalId, new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId), Provenance);
        }
    }
}