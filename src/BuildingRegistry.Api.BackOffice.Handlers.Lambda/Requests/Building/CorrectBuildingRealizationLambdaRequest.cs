namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record CorrectBuildingRealizationLambdaRequest : BuildingLambdaRequest, Abstractions.IHasBuildingPersistentLocalId
    {
        public CorrectBuildingRealizationRequest Request { get; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public CorrectBuildingRealizationLambdaRequest(
            string messageGroupId,
            CorrectBuildingRealizationSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to CorrectBuildingRealization command
        /// </summary>
        /// <returns>CorrectBuildingRealization.</returns>
        public CorrectBuildingRealization ToCommand()
        {
            return new CorrectBuildingRealization(new BuildingPersistentLocalId(BuildingPersistentLocalId), CommandProvenance);
        }
    }
}
