namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Requests.Building
{
    using Abstractions;
    using Abstractions.Building.Requests;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Commands;

    public sealed class SqsLambdaBuildingPlaceUnderConstructionRequest :
        SqsLambdaBuildingRequest,
        IHasBackOfficeRequest<BuildingBackOfficePlaceUnderConstructionRequest>,
        Abstractions.IHasBuildingPersistentLocalId
    {
        public BuildingBackOfficePlaceUnderConstructionRequest Request { get; set; }
        public int BuildingPersistentLocalId { get; set; }

        /// <summary>
        /// Map to PlaceBuildingUnderConstruction command
        /// </summary>
        /// <returns>PlaceBuildingUnderConstruction.</returns>
        public PlaceBuildingUnderConstruction ToCommand()
        {
            return new PlaceBuildingUnderConstruction(new BuildingPersistentLocalId(BuildingPersistentLocalId), Provenance);
        }
    }
}
