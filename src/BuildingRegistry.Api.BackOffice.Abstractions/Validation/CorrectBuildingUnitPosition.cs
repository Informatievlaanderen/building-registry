using TicketingService.Abstractions;

namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectBuildingUnitPosition
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwStatusNietInGeplandInAanbouwOfGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan binnen een gepland, inAanbouw of gerealiseerd gebouw.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingUnitInvalidStatus
            {
                public const string Code = "GebouweenheidNietGerealiseerdOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland' of 'gerealiseerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingUnitPositionOutsideBuildingGeometry
            {
                public const string Code = "GebouweenheidOngeldigePositieValidatie";
                public const string Message = "De positie dient binnen de geometrie van het gebouw te liggen.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
