namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Runtime.Serialization;

    [DataContract(Name = "NietRealiseerGebouweenheid", Namespace = "")]
    public class BackOfficeNotRealizeBuildingUnitRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }
    }
}
