namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using Abstractions.BuildingUnit.Converters;
    using Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class SqsLambdaBuildingUnitPlanRequest :
        SqsLambdaBuildingUnitRequest,
        IHasBackOfficeRequest<BuildingUnitBackOfficePlanRequest>
    {
        public BuildingUnitBackOfficePlanRequest Request { get; set; }

        /// <summary>
        /// Map to PlanBuildingUnit command
        /// </summary>
        /// <returns>PlanBuildingUnit.</returns>
        public PlanBuildingUnit ToCommand(
            BuildingPersistentLocalId buildingPersistentLocalId,
            BuildingUnitPersistentLocalId buildingUnitPersistentLocalId)
        {
            return new PlanBuildingUnit(
                buildingPersistentLocalId,
                buildingUnitPersistentLocalId,
                Request.PositieGeometrieMethode.Map(),
                new ExtendedWkbGeometry(Request.Positie),
                Request.Functie.Map(),
                Request.AfwijkingVastgesteld,
                Provenance);
        }
    }
}
