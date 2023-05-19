namespace BuildingRegistry.Grb.Abstractions
{
    public enum GrbEventType
    {
        Unknown = 0,
        DefineBuilding = 1,
        MeasureBuilding = 2,
        ChangeBuildingMeasurement = 3,
        CorrectBuildingMeasurement = 4,
        DemolishBuilding = 5
    }
}
