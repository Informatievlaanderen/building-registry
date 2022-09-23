namespace BuildingRegistry.Api.BackOffice.Abstractions.Building.Validators
{
    public static class ValidationErrorCodes
    {
        public static class Building
        {
            public const string BuildingNotFound = "GebouwNietGevonden";
            public const string BuildingRemoved = "GebouwIsVerwijderd";
            public const string InvalidPolygonGeometry = "GebouwPolygoonValidatie";
            public const string BuildingCannotBePlacedUnderConstruction = "GebouwGehistoreerdGerealiseerdOfNietGerealiseerd";
            public const string BuildingCannotBeRealizedException = "GebouwGehistoreerdGeplandOfNietGerealiseerd";
            public const string BuildingCannotBeNotRealizedException = "GebouwGehistoreerdOfGerealiseerd";
        }

        public static class BuildingUnit
        {
            public const string BuildingUnitNotFound = "GebouweenheidNietGevonden";
            public const string BuildingUnitIsRemoved = "GebouweenheidVerwijderd";
            public const string BuildingNotFound = "GebouweenheidGebouwIdNietGekendValidatie";
            public const string BuildingStatusNotInRealized = "GebouwStatusNietInGerealiseerd";
            public const string BuildingUnitCannotBeRealized = "GebouweenheidGehistoreerdOfNietGerealiseerd";
            public const string BuildingUnitCannotBeNotRealized = "GebouweenheidGehistoreerdOfGerealiseerd";
            public const string BuildingUnitCannotBePlanned = "GebouweenheidGebouwIdNietGerealiseerdofGehistoreerd";
            public const string BuildingUnitOutsideGeometryBuilding = "GebouweenheidOngeldigePositieValidatie";
            public const string MissingRequiredPosition = "GebouweendheidPositieValidatie";
            public const string InvalidPositionFormat = "GebouweenheidPositieformaatValidatie";
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
            public const string BuildingCannotBeNotRealizedException = "Deze actie is enkel toegestaan op gebouwen met status 'gepland' of 'inAanbouw'.";
        }

        public static class BuildingUnit
        {
            public static string BuildingInvalid(string buildingPuri)
                => $"De gebouwId '{buildingPuri}' is niet gekend in het gebouwenregister.";

            public const string BuildingNotFound = "Onbestaand gebouw.";
            public const string BuildingUnitNotFound = "Onbestaande gebouweenheid.";
            public const string BuildingUnitIsRemoved = "Verwijderde gebouweenheid.";
            public const string BuildingStatusNotInRealized = "Deze actie is enkel toegestaan binnen een gerealiseerd gebouw.";
            public const string BuildingUnitCannotBeRealized = "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland'.";
            public const string BuildingUnitCannotBeNotRealized = "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland'.";
            public const string BuildingUnitCannotBePlanned = "De gebouwId is niet gerealiseerd of gehistoreerd.";
            public const string BuildingUnitOutsideGeometryBuilding = "De positie dient binnen de geometrie van het gebouw te liggen.";
            public const string MissingRequiredPosition = "De verplichte parameter 'positie' ontbreekt.";
            public const string InvalidPositionFormat = "De positie is geen geldige gml-puntgeometrie.";
        }
    }
}
