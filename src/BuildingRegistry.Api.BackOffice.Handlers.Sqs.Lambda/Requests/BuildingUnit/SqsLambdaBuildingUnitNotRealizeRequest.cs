namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class SqsLambdaBuildingUnitNotRealizeRequest :
        SqsLambdaBuildingUnitRequest,
        IHasBackOfficeRequest<BuildingUnitBackOfficeNotRealizeRequest>,
        IHasBuildingUnitPersistentLocalId
    {
        public BuildingUnitBackOfficeNotRealizeRequest Request { get; set; }

        public int BuildingUnitPersistentLocalId => Request.BuildingUnitPersistentLocalId;

        /// <summary>
        /// Map to NotRealizeBuildingUnit command
        /// </summary>
        /// <returns>NotRealizeBuildingUnit.</returns>
        public NotRealizeBuildingUnit ToCommand()
        {
            return new NotRealizeBuildingUnit(
                BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                Provenance);
        }
    }
}
