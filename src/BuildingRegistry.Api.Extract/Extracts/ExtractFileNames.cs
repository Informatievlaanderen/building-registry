namespace BuildingRegistry.Api.Extract.Extracts
{
    using System;

    internal class ExtractFileNames
    {
        public static string Building => "Gebouw";
        public static string BuildingUnit => "Gebouweenheid";

        public static string GetBuildingZipName() => $"Gebouw-{DateTime.Today:yyyy-MM-dd}";
    }
}
