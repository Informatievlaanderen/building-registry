namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    public static class ValidationErrorCodes
    {
        public static string InvalidPolygonGeometry = "GebouwPolygoonValidatie";
        public static string BuildingCannotBePlacedUnderConstruction = "GebouwGehistoreerdGerealiseerdOfNietGerealiseerd";
        public static string BuildingCannotBeRealizedException = "GebouwGehistoreerdGeplandOfNietGerealiseerd";
    }
}
