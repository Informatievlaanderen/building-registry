namespace BuildingRegistry.Api.BackOffice.Abstractions.BuildingUnit.Extensions
{
    public static class PuriCreator
    {
        public static string CreateAdresId(int persistentLocalId) => $"https://data.vlaanderen.be/id/adressen/{persistentLocalId}";
    }
}
