namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class SqsLambdaBuildingPlanRequest :
        SqsLambdaBuildingRequest,
        IHasBackOfficeRequest<BuildingBackOfficePlanRequest>
    {
        public BuildingBackOfficePlanRequest Request { get; set; }

        /// <summary>
        /// Map to PlanBuilding command
        /// </summary>
        /// <returns>PlanBuilding.</returns>
        public PlanBuilding ToCommand(BuildingPersistentLocalId buildingPersistentLocalId)
        {
            return new PlanBuilding(buildingPersistentLocalId, new ExtendedWkbGeometry(Request.GeometriePolygoon), Provenance);
        }
    }
}
