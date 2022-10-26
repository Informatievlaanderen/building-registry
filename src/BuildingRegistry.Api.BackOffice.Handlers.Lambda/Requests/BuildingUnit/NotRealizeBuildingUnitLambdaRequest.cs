namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class NotRealizeBuildingUnitLambdaRequest :
        BuildingUnitLambdaRequest,
        IHasBackOfficeRequest<NotRealizeBuildingUnitBackOfficeRequest>,
        IHasBuildingUnitPersistentLocalId
    {
        public NotRealizeBuildingUnitLambdaRequest(
            Guid ticketId,
            string messageGroupId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object> metadata,
            NotRealizeBuildingUnitBackOfficeRequest request)
            : base(ticketId, messageGroupId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

        public NotRealizeBuildingUnitBackOfficeRequest Request { get; set; }

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
