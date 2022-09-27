namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;

    public class BackOfficeRealizeBuildingRequest
    {
        public int PersistentLocalId { get; set; }
    }
}
