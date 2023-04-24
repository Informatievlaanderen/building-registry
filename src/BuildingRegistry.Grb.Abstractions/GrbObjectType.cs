namespace BuildingRegistry.Grb.Abstractions
{
    public enum GrbObjectType
    {
        Unknown = 0,
        MainBuilding = 1, // hoofdgebouw
        OutBuilding = 2, // bijgebouw
        BuildingLinedWithVirtualFacades = 3, // gebouw afgezoomd met virtuele gevels
    }
}
