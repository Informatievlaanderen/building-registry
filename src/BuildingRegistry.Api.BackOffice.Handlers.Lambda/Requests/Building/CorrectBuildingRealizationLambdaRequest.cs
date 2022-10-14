namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class CorrectBuildingRealizationLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<BackOfficeCorrectBuildingRealizationRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public BackOfficeCorrectBuildingRealizationRequest Request { get; set; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectBuildingRealization command
        /// </summary>
        /// <returns>CorrectBuildingRealization.</returns>
        public CorrectBuildingRealization ToCommand()
        {
            return new CorrectBuildingRealization(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
