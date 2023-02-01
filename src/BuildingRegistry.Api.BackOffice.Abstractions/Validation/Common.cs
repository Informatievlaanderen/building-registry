namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class Common
        {
            public static class BuildingNotFound
            {
                public const string Code = "GebouweenheidGebouwIdNietGekendValidatie";
                public const string Message = "Onbestaand gebouw.";

                public static string InvalidGebouwId(string buildingPuri) => $"De gebouwId '{buildingPuri}' is niet gekend in het gebouwenregister.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingIsRemoved
            {
                public const string Code = "GebouwIsVerwijderd";
                public const string Message = "Verwijderd gebouw.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingUnitNotFound
            {
                public const string Code = "GebouweenheidNietGevonden";
                public const string Message = "Onbestaande gebouweenheid.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingUnitIsRemoved
            {
                public const string Code = "VerwijderdeGebouweenheid";
                public const string Message = "Verwijderde gebouweenheid.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingUnitHasInvalidFunction
            {
                public const string Code = "GebouweenheidGemeenschappelijkDeel";
                public const string Message = "Deze actie is niet toegestaan op gebouweenheden met functie gemeenschappelijkDeel.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingUnitRequiredPosition
            {
                public const string Code = "GebouweendheidPositieValidatie";
                public const string Message = "De verplichte parameter 'positie' ontbreekt.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class CommonBuildingUnit
            {
                public static class InvalidFunction
                {
                    public const string Code = "GebouweenheidGemeenschappelijkDeel";
                    public const string Message = "Deze actie is niet toegestaan op gebouweenheden met functie gemeenschappelijkDeel.";

                    public static TicketError ToTicketError() => new(Message, Code);
                }
            }

            public static class AdresIdInvalid
            {
                public const string Code = "GebouweenheidAdresOngeldig";
                public const string Message = "Ongeldig adresId.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class InvalidBuildingPolygonGeometry
            {
                public const string Code = "GebouwPolygoonValidatie";
                public const string Message = "Ongeldig formaat geometriePolygoon.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class InvalidBuildingUnitPosition
            {
                public const string Code = "GebouweenheidPositieformaatValidatie";
                public const string Message = "De positie is geen geldige gml-puntgeometrie.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingIsMeasuredByGrb
            {
                public const string Code = "GebouwGeometrieMethodeIngemetenGRB";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met geometrieMethode 'ingeschetst'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
