namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record CorrectBuildingNotRealizationLambdaRequest : BuildingLambdaRequest, Abstractions.IHasBuildingPersistentLocalId
    {
        public CorrectBuildingNotRealizationRequest Request { get; }

        public int BuildingPersistentLocalId => Request.PersistentLocalId;

        public CorrectBuildingNotRealizationLambdaRequest(
            string messageGroupId,
            CorrectBuildingNotRealizationSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to CorrectBuildingNotRealization command
        /// </summary>
        /// <returns>CorrectBuildingNotRealization.</returns>
        public CorrectBuildingNotRealization ToCommand()
        {
            return new CorrectBuildingNotRealization(new BuildingPersistentLocalId(BuildingPersistentLocalId), CommandProvenance);
        }
    }
}
