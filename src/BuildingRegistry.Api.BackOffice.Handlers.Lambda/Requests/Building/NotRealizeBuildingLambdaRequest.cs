namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class NotRealizeBuildingLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<BackOfficeNotRealizeBuildingRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public BackOfficeNotRealizeBuildingRequest Request { get; set; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to NotRealizeBuilding command.
        /// </summary>
        /// <returns>NotRealizeBuilding.</returns>
        public NotRealizeBuilding ToCommand()
        {
            return new NotRealizeBuilding(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
