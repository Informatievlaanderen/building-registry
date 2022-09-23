namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Runtime.Serialization;

    [DataContract(Name = "RealiseerGebouweenheid", Namespace = "")]
    public sealed class BuildingUnitBackOfficeRealizeRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }
    }
}
