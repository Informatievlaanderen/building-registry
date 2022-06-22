namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    public static class ValidationErrorCodes
    {
        public static string InvalidGeometry = "InvalidGeometry";
        public static string BuildingCannotBePlacedUnderConstruction = "GebouwStatusNietGepland";
        public static string BuildingCannotBeRealizedException = "GebouwStatusInAanbouw";
    }
}
