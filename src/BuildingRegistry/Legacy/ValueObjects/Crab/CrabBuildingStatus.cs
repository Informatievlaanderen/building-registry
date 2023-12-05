namespace BuildingRegistry.Legacy.Crab
{
    using System;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public enum CrabBuildingStatus
    {
        PermitRequested = 1,
        BuildingPermitGranted = 2,
        UnderConstruction = 3,
        InUse = 4,
        OutOfUse = 5
    }
}
