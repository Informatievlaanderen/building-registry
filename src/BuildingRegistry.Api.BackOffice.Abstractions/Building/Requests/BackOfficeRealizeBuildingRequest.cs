namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;

    [DataContract(Name = "RealiseerGebouw", Namespace = "")]
    public class BackOfficeRealizeBuildingRequest
    {
        public int PersistentLocalId { get; set; }
    }
}
