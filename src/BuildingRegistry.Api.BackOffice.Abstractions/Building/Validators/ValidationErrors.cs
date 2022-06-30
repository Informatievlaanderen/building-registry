namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    public static class ValidationErrorCodes
    {
        public static class Building
        {
            public const string BuildingNotFound = "GebouwNietGevonden";
            public const string InvalidPolygonGeometry = "GebouwPolygoonValidatie";
            public const string BuildingCannotBePlacedUnderConstruction = "GebouwGehistoreerdGerealiseerdOfNietGerealiseerd";
            public const string BuildingCannotBeRealizedException = "GebouwGehistoreerdGeplandOfNietGerealiseerd";
        }

        public static class BuildingUnit
        {
            public const string BuildingUnitNotFound = "GebouweenheidNietGevonden";
            public const string BuildingUnitIsRemoved = "GebouweenheidVerwijderd";
        }
    }

    public static class ValidationErrorMessages
    {
        public static class Building
        {
            public const string BuildingNotFound = "Onbestaand gebouw.";
            public const string BuildingRemoved = "Verwijderd gebouw.";
            public const string InvalidPolygonGeometry = "Ongeldig formaat geometriePolygoon.";
            public const string BuildingCannotBePlacedUnderConstruction = "Deze actie is enkel toegestaan op gebouwen met status 'gepland'.";
            public const string BuildingCannotBeRealizedException = "Deze actie is enkel toegestaan op gebouwen met status 'inAanbouw'.";
        }

        public static class BuildingUnit
        {
            public const string BuildingUnitNotFound = "Onbestaande gebouweenheid.";
            public const string BuildingUnitIsRemoved = "Verwijderde gebouweenheid.";
        }
    }
}
