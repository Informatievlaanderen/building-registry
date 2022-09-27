namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Runtime.Serialization;

    public class BackOfficeNotRealizeBuildingUnitRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }
    }
}
