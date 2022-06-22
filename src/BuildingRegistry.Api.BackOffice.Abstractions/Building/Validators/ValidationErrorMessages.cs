namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    public static class ValidationErrorMessages
    {
        public static string InvalidGeometry = "InvalidGeometry";
        public static string BuildingRemoved = "Verwijderd gebouw.";
        public static string BuildingNotFound = "Onbestaand gebouw.";
        public static string BuildingCannotBePlacedUnderConstruction = "Deze actie is enkel toegestaan op gebouwen met status 'gepland'.";
        public static string BuildingCannotBeRealizedException = "Deze actie is enkel toegestaan op gebouwen met status 'inAanbouw'.";
    }
}
