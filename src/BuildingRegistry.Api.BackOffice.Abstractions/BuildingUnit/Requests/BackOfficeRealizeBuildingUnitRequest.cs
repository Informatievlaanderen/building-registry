namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Runtime.Serialization;

    public class BackOfficeRealizeBuildingUnitRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }
    }
}
