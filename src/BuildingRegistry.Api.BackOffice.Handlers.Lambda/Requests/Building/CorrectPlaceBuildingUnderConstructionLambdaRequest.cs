namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class CorrectPlaceBuildingUnderConstructionLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<BackOfficeCorrectPlaceBuildingUnderConstructionRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public BackOfficeCorrectPlaceBuildingUnderConstructionRequest Request { get; set; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public CorrectPlaceBuildingUnderConstructionLambdaRequest(
            Guid ticketId,
            string messageGroupId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object> metadata,
            BackOfficeCorrectPlaceBuildingUnderConstructionRequest request)
            : base(ticketId, messageGroupId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        /// <summary>
        /// Map to CorrectBuildingPlaceUnderConstruction command.
        /// </summary>
        /// <returns>CorrectBuildingPlaceUnderConstruction.</returns>
        public CorrectBuildingPlaceUnderConstruction ToCommand()
        {
            return new CorrectBuildingPlaceUnderConstruction(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
