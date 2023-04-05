namespace BuildingRegistry.Api.Extract.Extracts
{
    using System;

    public static class ExtractFileNames
    {
        public const string BuildingExtractZipName = "Gebouw";
        public const string BuildingUnitExtractZipName = "Gebouweenheid";
        public const string AddressLinkExtractZipName = "Adreskoppelingen";

        public static string GetBuildingZipName() => $"Gebouw-{DateTime.Today:yyyy-MM-dd}";
        public static string AddressLinkExtractFileName => $"{AddressLinkExtractZipName}-{DateTime.Today:yyyy-MM-dd}";
    }
}
