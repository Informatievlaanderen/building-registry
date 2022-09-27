namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class PlanBuildingLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<BackOfficePlanBuildingRequest>
    {
        public BackOfficePlanBuildingRequest Request { get; set; }

        /// <summary>
        /// Map to PlanBuilding command
        /// </summary>
        /// <returns>PlanBuilding.</returns>
        public PlanBuilding ToCommand(BuildingPersistentLocalId buildingPersistentLocalId)
        {
            return new PlanBuilding(
                buildingPersistentLocalId,
                Request.GeometriePolygoon.ToExtendedWkbGeometry(),
                Provenance);
        }
    }
}
