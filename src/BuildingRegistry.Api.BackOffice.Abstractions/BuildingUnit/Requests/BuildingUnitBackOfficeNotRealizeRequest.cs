namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Runtime.Serialization;

    [DataContract(Name = "NietRealiseerGebouweenheid", Namespace = "")]
    public sealed class BuildingUnitBackOfficeNotRealizeRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }
    }
}
