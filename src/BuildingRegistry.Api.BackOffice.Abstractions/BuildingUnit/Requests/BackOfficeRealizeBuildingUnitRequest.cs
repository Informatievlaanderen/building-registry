namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Runtime.Serialization;

    [DataContract(Name = "RealiseerGebouweenheid", Namespace = "")]
    public class BackOfficeRealizeBuildingUnitRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }
    }
}
