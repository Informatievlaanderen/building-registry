namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Requests
{
    using System.Runtime.Serialization;

    public class BackOfficeCorrectBuildingUnitRealizationRequest
    {
        public int BuildingUnitPersistentLocalId { get; set; }
    }
}
