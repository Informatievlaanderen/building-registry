namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;

    [DataContract(Name = "NietRealiseerGebouw", Namespace = "")]
    public class BackOfficeNotRealizeBuildingRequest
    {
        public int PersistentLocalId { get; set; }
    }
}
