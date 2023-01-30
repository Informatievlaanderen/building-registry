namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions.BuildingUnit.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using IHasBuildingUnitPersistentLocalId = Abstractions.IHasBuildingUnitPersistentLocalId;

    public sealed record CorrectBuildingUnitNotRealizationLambdaRequest :
        BuildingUnitLambdaRequest,
        IHasBuildingUnitPersistentLocalId
    {
        public CorrectBuildingUnitNotRealizationRequest Request { get; }

        public int BuildingUnitPersistentLocalId => Request.BuildingUnitPersistentLocalId;

        public CorrectBuildingUnitNotRealizationLambdaRequest(
            string messageGroupId,
            CorrectBuildingUnitNotRealizationSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public CorrectBuildingUnitNotRealizationLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            CorrectBuildingUnitNotRealizationRequest request)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        /// <summary>
        /// Map to CorrectBuildingUnitNotRealization command.
        /// </summary>
        /// <returns>CorrectBuildingUnitNotRealization.</returns>
        public CorrectBuildingUnitNotRealization ToCommand()
        {
            return new CorrectBuildingUnitNotRealization(
                BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                Provenance);
        }
    }
}
