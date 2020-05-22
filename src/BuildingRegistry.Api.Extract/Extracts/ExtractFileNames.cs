namespace BuildingRegistry.Api.Extract.Extracts
{
    using System;

    internal class ExtractFileNames
    {
        public const string Building = "Gebouw";
        public const string BuildingUnit = "Gebouweenheid";

        public static string GetBuildingZipName() => $"Gebouw-{DateTime.Today:yyyy-MM-dd}";
    }
}
