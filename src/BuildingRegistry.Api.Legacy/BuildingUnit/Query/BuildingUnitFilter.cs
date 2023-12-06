namespace BuildingRegistry.Api.Legacy.BuildingUnit.Query
{
    public class BuildingUnitFilter
    {
        public int? BuildingPersistentLocalId { get; set; }
        public string AddressPersistentLocalId { get; set; }
        public string Status { get; set; }
        public string? Functie { get; set; }
    }
}
