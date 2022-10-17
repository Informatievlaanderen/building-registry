namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class CorrectBuildingUnitRealizationLambdaRequest :
        BuildingUnitLambdaRequest,
        IHasBackOfficeRequest<BackOfficeCorrectBuildingUnitRealizationRequest>,
        IHasBuildingUnitPersistentLocalId
    {
        public CorrectBuildingUnitRealizationLambdaRequest(
            Guid ticketId,
            string messageGroupId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object> metadata,
            BackOfficeCorrectBuildingUnitRealizationRequest request)
            : base(ticketId, messageGroupId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        public BackOfficeCorrectBuildingUnitRealizationRequest Request { get; set; }

        public int BuildingUnitPersistentLocalId => Request.BuildingUnitPersistentLocalId;

        /// <summary>
        /// Map to CorrectBuildingUnitRealization command
        /// </summary>
        /// <returns>CorrectBuildingUnitRealization.</returns>
        public CorrectBuildingUnitRealization ToCommand()
        {
            return new CorrectBuildingUnitRealization(BuildingPersistentLocalId, new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId), Provenance);
        }
    }
}
