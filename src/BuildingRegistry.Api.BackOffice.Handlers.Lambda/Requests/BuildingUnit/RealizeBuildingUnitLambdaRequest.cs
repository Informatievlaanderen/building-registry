namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class RealizeBuildingUnitLambdaRequest :
        BuildingUnitLambdaRequest,
        IHasBackOfficeRequest<RealizeBuildingUnitBackOfficeRequest>,
        IHasBuildingUnitPersistentLocalId
    {
        public RealizeBuildingUnitLambdaRequest(
            Guid ticketId,
            string messageGroupId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object> metadata,
            RealizeBuildingUnitBackOfficeRequest request)
            : base(ticketId, messageGroupId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        public RealizeBuildingUnitBackOfficeRequest Request { get; set; }

        public int BuildingUnitPersistentLocalId => Request.BuildingUnitPersistentLocalId;

        /// <summary>
        /// Map to RealizeBuildingUnit command
        /// </summary>
        /// <returns>RealizeBuildingUnit.</returns>
        public RealizeBuildingUnit ToCommand()
        {
            return new RealizeBuildingUnit(BuildingPersistentLocalId, new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId), Provenance);
        }
    }
}
