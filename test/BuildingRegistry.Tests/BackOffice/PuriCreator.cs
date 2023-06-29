namespace BuildingRegistry.Tests.BackOffice
{
    public static class PuriCreator
    {
        public static string CreateAdresId(int persistentLocalId) => $"https://data.vlaanderen.be/id/adres/{persistentLocalId}";
        public static string CreateBuildingId(int persistentLocalId) => $"https://data.vlaanderen.be/id/gebouw/{persistentLocalId}";
    }
}
