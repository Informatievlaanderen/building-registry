namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    public static class ValidationErrorMessages
    {
        public static string InvalidPolygonGeometry = "Ongeldig formaat geometriePolygoon.";
        public static string BuildingRemoved = "Verwijderd gebouw.";
        public static string BuildingNotFound = "Onbestaand gebouw.";
        public static string BuildingUnitNotFound = "Onbestaande gebouweenheid.";
        public static string BuildingCannotBePlacedUnderConstruction = "Deze actie is enkel toegestaan op gebouwen met status 'gepland'.";
        public static string BuildingCannotBeRealizedException = "Deze actie is enkel toegestaan op gebouwen met status 'inAanbouw'.";
    }
}
