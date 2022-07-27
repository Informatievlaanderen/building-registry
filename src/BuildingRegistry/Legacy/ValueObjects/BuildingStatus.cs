namespace BuildingRegistry.Legacy
{
    using System;
    public enum BuildingStatus
    {
        Planned = 0,
        UnderConstruction = 1,
        Realized = 2,
        Retired = 3,
        NotRealized = 4
    }

    public static class BuildingStatusHelpers
    {
        public static BuildingRegistry.Building.BuildingStatus Map(this BuildingStatus status)
        {
            return status switch
            {
                BuildingStatus.Planned => BuildingRegistry.Building.BuildingStatus.Planned,
                BuildingStatus.UnderConstruction => BuildingRegistry.Building.BuildingStatus.UnderConstruction,
                BuildingStatus.Realized => BuildingRegistry.Building.BuildingStatus.Realized,
                BuildingStatus.Retired => BuildingRegistry.Building.BuildingStatus.Retired,
                BuildingStatus.NotRealized => BuildingRegistry.Building.BuildingStatus.NotRealized,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, $"Non existing status '{status}'.")
            };
        }
    }
}
