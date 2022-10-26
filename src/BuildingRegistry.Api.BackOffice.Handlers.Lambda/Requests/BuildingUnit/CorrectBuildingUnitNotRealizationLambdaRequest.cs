namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class CorrectBuildingUnitNotRealizationLambdaRequest :
        BuildingUnitLambdaRequest,
        IHasBackOfficeRequest<CorrectBuildingUnitNotRealizationBackOfficeRequest>,
        IHasBuildingUnitPersistentLocalId
    {
        public CorrectBuildingUnitNotRealizationBackOfficeRequest Request { get; set; }

        public int BuildingUnitPersistentLocalId => Request.BuildingUnitPersistentLocalId;

        public CorrectBuildingUnitNotRealizationLambdaRequest(
            Guid ticketId,
            string messageGroupId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object> metadata,
            CorrectBuildingUnitNotRealizationBackOfficeRequest request)
            : base(ticketId, messageGroupId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        /// <summary>
        /// Map to CorrectBuildingUnitNotRealization command.
        /// </summary>
        /// <returns>CorrectBuildingUnitNotRealization.</returns>
        public CorrectBuildingUnitNotRealization ToCommand()
        {
            return new CorrectBuildingUnitNotRealization(
                BuildingPersistentLocalId,
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                Provenance);
        }
    }
}
