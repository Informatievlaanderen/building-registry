namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;

    public class BackOfficeNotRealizeBuildingRequest
    {
        public int PersistentLocalId { get; set; }
    }
}
