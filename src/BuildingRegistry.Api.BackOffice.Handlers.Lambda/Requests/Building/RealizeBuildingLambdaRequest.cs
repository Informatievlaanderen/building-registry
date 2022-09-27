namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class RealizeBuildingLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<BackOfficeRealizeBuildingRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public BackOfficeRealizeBuildingRequest Request { get; set; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to RealizeBuilding command
        /// </summary>
        /// <returns>RealizeBuilding.</returns>
        public RealizeBuilding ToCommand()
        {
            return new RealizeBuilding(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
