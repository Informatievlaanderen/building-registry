namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class RealizeBuildingUnitLambdaRequest :
        BuildingUnitLambdaRequest,
        IHasBackOfficeRequest<BackOfficeRealizeBuildingUnitRequest>,
        IHasBuildingUnitPersistentLocalId
    {
        public BackOfficeRealizeBuildingUnitRequest Request { get; set; }

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
