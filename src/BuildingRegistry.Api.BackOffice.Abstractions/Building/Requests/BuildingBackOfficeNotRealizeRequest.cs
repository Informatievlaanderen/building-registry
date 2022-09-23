namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Requests
{
    using System.Runtime.Serialization;

    [DataContract(Name = "NietRealiseerGebouw", Namespace = "")]
    public sealed class BuildingBackOfficeNotRealizeRequest
    {
        public int PersistentLocalId { get; set; }
    }
}
