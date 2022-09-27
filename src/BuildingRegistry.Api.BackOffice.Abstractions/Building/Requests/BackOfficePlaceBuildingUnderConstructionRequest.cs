namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;

    public class BackOfficePlaceBuildingUnderConstructionRequest
    {
        public int PersistentLocalId { get; set; }
    }
}
