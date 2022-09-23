namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;

    [DataContract(Name = "RealiseerGebouw", Namespace = "")]
    public sealed class BuildingBackOfficeRealizeRequest
    {
        public int PersistentLocalId { get; set; }
    }
}
