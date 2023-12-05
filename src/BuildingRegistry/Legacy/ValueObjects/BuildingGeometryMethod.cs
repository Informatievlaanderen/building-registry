namespace BuildingRegistry.Legacy
{
    using System;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public enum BuildingGeometryMethod
    {
        Outlined,
        MeasuredByGrb
    }

    public static class BuildingGeometryMethodHelpers
    {
        public static BuildingRegistry.Building.BuildingGeometryMethod Map(this BuildingGeometryMethod geometryMethod)
        {
            return geometryMethod switch
            {
                BuildingGeometryMethod.Outlined => BuildingRegistry.Building.BuildingGeometryMethod.Outlined,
                BuildingGeometryMethod.MeasuredByGrb => BuildingRegistry.Building.BuildingGeometryMethod.MeasuredByGrb,
                _ => throw new ArgumentOutOfRangeException(nameof(geometryMethod), geometryMethod, $"Non existing geometrymethod '{geometryMethod}'.")
            };
        }
    }
}
