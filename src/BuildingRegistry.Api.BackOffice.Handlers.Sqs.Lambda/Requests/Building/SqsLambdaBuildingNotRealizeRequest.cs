namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class SqsLambdaBuildingNotRealizeRequest :
        SqsLambdaBuildingRequest,
        IHasBackOfficeRequest<BuildingBackOfficeNotRealizeRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public BuildingBackOfficeNotRealizeRequest Request { get; set; }

        public int BuildingPersistentLocalId { get; set; }

        /// <summary>
        /// Map to NotRealizeBuilding command
        /// </summary>
        /// <returns>NotRealizeBuilding.</returns>
        public NotRealizeBuilding ToCommand()
        {
            return new NotRealizeBuilding(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
