namespace BuildingRegistry.Infrastructure
{
    public static class PersistentLocalIdHelper
    {
        private const string DataVlaanderenUrl = "https://data.vlaanderen.be/id";

        public static string CreateBuildingId(int id)
        {
            return $"{DataVlaanderenUrl}/gebouw/{id}";
        }

        public static string CreateBuildingUnitId(int id)
        {
            return $"{DataVlaanderenUrl}/gebouweenheid/{id}";
        }
    }
}
