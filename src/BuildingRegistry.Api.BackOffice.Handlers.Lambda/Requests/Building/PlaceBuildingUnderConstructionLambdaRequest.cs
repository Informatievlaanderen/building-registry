namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class PlaceBuildingUnderConstructionLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<BackOfficePlaceBuildingUnderConstructionRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public BackOfficePlaceBuildingUnderConstructionRequest Request { get; set; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public PlaceBuildingUnderConstructionLambdaRequest(
            Guid ticketId,
            string messageGroupId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object> metadata,
            BackOfficePlaceBuildingUnderConstructionRequest request)
            : base(ticketId, messageGroupId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        /// <summary>
        /// Map to PlaceBuildingUnderConstruction command.
        /// </summary>
        /// <returns>PlaceBuildingUnderConstruction.</returns>
        public PlaceBuildingUnderConstruction ToCommand()
        {
            return new PlaceBuildingUnderConstruction(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
