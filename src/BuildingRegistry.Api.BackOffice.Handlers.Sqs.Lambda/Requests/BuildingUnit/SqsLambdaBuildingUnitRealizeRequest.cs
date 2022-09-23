namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class SqsLambdaBuildingUnitRealizeRequest :
        SqsLambdaBuildingUnitRequest,
        IHasBackOfficeRequest<BuildingUnitBackOfficeRealizeRequest>,
        IHasBuildingUnitPersistentLocalId
    {
        public BuildingUnitBackOfficeRealizeRequest Request { get; set; }

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
