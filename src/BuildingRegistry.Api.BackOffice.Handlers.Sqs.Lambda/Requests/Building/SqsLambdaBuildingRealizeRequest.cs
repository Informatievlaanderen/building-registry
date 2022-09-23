namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class SqsLambdaBuildingRealizeRequest :
        SqsLambdaBuildingRequest,
        IHasBackOfficeRequest<BuildingBackOfficeRealizeRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public BuildingBackOfficeRealizeRequest Request { get; set; }

        public int BuildingPersistentLocalId { get; set; }

        /// <summary>
        /// Map to RealizeBuilding command
        /// </summary>
        /// <returns>RealizeBuilding.</returns>
        public RealizeBuilding ToCommand()
        {
            return new RealizeBuilding(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }

    }
}
