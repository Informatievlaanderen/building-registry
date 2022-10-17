namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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

        public CorrectBuildingNotRealizationLambdaRequest(
            Guid ticketId,
            string messageGroupId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object> metadata,
            BackOfficeCorrectBuildingNotRealizationRequest request)
            : base(ticketId, messageGroupId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

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
