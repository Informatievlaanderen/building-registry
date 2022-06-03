namespace BuildingRegistry.Api.Extract.Abstractions.Extracts
{
    using System;

    public static class ExtractFileNames
    {
        public const string Building = "Gebouw";
        public const string BuildingUnit = "Gebouweenheid";

        public static string GetBuildingZipName() => $"Gebouw-{DateTime.Today:yyyy-MM-dd}";
    }
}
