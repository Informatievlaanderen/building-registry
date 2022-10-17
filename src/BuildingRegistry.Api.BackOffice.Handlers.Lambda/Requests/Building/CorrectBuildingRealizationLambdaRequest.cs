namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
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

        public CorrectBuildingRealizationLambdaRequest(
            Guid ticketId,
            string messageGroupId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object> metadata,
            BackOfficeCorrectBuildingRealizationRequest request)
            : base(ticketId, messageGroupId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

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
