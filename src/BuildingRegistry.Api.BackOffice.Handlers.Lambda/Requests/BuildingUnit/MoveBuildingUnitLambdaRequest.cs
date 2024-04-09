namespace BuildingRegistry.Api.BackOffice.Handlers.Lambda.Requests.BuildingUnit
{
    using Abstractions.BuildingUnit.Requests;
    using Abstractions.BuildingUnit.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed record MoveBuildingUnitLambdaRequest : BuildingUnitLambdaRequest
    {
        public MoveBuildingUnitRequest Request { get; }

        public int BuildingUnitPersistentLocalId { get; }

        public MoveBuildingUnitLambdaRequest(
            string messageGroupId,
            MoveBuildingUnitSqsRequest sqsRequest)
            : base(messageGroupId, sqsRequest.TicketId, sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(), sqsRequest.Metadata)
        {
            BuildingUnitPersistentLocalId = sqsRequest.BuildingUnitPersistentLocalId;
            Request = sqsRequest.Request;
        }

        public MoveBuildingUnitIntoBuilding ToMoveBuildingUnitIntoBuildingCommand()
        {
            return new MoveBuildingUnitIntoBuilding(
                BuildingPersistentLocalId,
                new BuildingPersistentLocalId(Convert.ToInt32(Request.DoelgebouwId.AsIdentifier().Map(x => x).Value)),
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                Provenance);
        }

        public MoveBuildingUnitOutOfBuilding ToMoveBuildingUnitOutOfBuildingCommand()
        {
            return new MoveBuildingUnitOutOfBuilding(
                BuildingPersistentLocalId,
                new BuildingPersistentLocalId(Convert.ToInt32(Request.DoelgebouwId.AsIdentifier().Map(x => x).Value)),
                new BuildingUnitPersistentLocalId(BuildingUnitPersistentLocalId),
                Provenance);
        }
    }
}
