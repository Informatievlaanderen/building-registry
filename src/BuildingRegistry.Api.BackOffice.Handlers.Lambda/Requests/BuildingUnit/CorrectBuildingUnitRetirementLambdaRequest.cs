namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions.BuildingUnit.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using IHasBuildingUnitPersistentLocalId = Abstractions.IHasBuildingUnitPersistentLocalId;

    public sealed record CorrectBuildingUnitRetirementLambdaRequest : BuildingUnitLambdaRequest, IHasBuildingUnitPersistentLocalId
    {
        public CorrectBuildingUnitRetirementRequest Request { get; }

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
            CorrectBuildingUnitRetirementRequest request)
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
