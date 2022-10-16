namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using Sqs.Requests.Building;

    public sealed record NotRealizeBuildingLambdaRequest :
        BuildingLambdaRequest,
        IHasBackOfficeRequest<NotRealizeBuildingBackOfficeRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public NotRealizeBuildingBackOfficeRequest Request { get; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public NotRealizeBuildingLambdaRequest(string messageGroupId, NotRealizeBuildingSqsRequest sqsRequest)
            : this(
                messageGroupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata,
                sqsRequest.Request)
        { }

        public NotRealizeBuildingLambdaRequest(
            string messageGroupId,
            Guid ticketId,
            string? ifMatchHeaderValue,
            Provenance provenance,
            IDictionary<string, object?> metadata,
            NotRealizeBuildingBackOfficeRequest request)
            : base(messageGroupId, ticketId, ifMatchHeaderValue, provenance, metadata)
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
