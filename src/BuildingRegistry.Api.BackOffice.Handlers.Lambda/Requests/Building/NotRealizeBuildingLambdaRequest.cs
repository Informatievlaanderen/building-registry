namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class NotRealizeBuildingLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<NotRealizeBuildingBackOfficeRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public NotRealizeBuildingBackOfficeRequest Request { get; set; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public NotRealizeBuildingLambdaRequest(
            Guid ticketId,
            string messageGroupId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object> metadata,
            NotRealizeBuildingBackOfficeRequest request)
            : base(ticketId, messageGroupId, ifMatchHeaderValue, provenance, metadata)
        {
            Request = request;
        }

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
