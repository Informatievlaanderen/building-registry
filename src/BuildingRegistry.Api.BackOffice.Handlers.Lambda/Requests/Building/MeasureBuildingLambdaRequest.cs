namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.Building
{
    using Abstractions.Building;
    using Abstractions.Building.Requests;
    using Abstractions.Building.SqsRequests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;
    using IHasBuildingPersistentLocalId = Abstractions.IHasBuildingPersistentLocalId;

    public sealed record MeasureBuildingLambdaRequest : BuildingLambdaRequest, IHasBuildingPersistentLocalId
    {
        public MeasureBuildingRequest Request { get; }

        public int BuildingPersistentLocalId { get; }

        public MeasureBuildingLambdaRequest(
            string messageGroupId,
            MeasureBuildingSqsRequest sqsRequest)
            : base(
                messageGroupId,
                sqsRequest.TicketId,
                ifMatchHeaderValue: null,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            BuildingPersistentLocalId = sqsRequest.BuildingPersistentLocalId;
            Request = sqsRequest.Request;
        }

        /// <summary>
        /// Map to MeasureBuilding command
        /// </summary>
        /// <returns>MeasureBuilding.</returns>
        public MeasureBuilding ToCommand()
        {
            return new MeasureBuilding(
                new BuildingPersistentLocalId(BuildingPersistentLocalId),
                Request.GrbData.GeometriePolygoon.ToExtendedWkbGeometry(),
                Request.GrbData.ToBuildingGrbData(),
                Provenance);
        }
    }
}
