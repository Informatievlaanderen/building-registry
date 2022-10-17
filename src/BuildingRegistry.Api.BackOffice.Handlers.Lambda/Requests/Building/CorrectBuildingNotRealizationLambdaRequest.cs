namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class CorrectBuildingNotRealizationLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<BackOfficeCorrectBuildingNotRealizationRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public BackOfficeCorrectBuildingNotRealizationRequest Request { get; set; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        /// <summary>
        /// Map to CorrectBuildingNotRealization command
        /// </summary>
        /// <returns>CorrectBuildingNotRealization.</returns>
        public CorrectBuildingNotRealization ToCommand()
        {
            return new CorrectBuildingNotRealization(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
