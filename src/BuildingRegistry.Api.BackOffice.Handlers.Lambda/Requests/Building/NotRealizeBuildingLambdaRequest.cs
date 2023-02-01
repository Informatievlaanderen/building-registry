namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record NotRealizeBuildingLambdaRequest : BuildingLambdaRequest, Abstractions.IHasBuildingPersistentLocalId
    {
        public NotRealizeBuildingRequest Request { get; }

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
            NotRealizeBuildingRequest request)
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
