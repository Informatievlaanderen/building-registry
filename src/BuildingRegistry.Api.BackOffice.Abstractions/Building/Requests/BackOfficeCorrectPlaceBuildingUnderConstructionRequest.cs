namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;

    public class BackOfficeCorrectPlaceBuildingUnderConstructionRequest
    {
        public int PersistentLocalId { get; set; }
    }
}
