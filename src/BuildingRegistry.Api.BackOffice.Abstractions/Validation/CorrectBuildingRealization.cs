using TicketingService.Abstractions;

namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectBuildingRealization
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwGeplandGehistoreerdOfNietGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met status 'gerealiseerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingHasRetiredBuildingUnits
            {
                public const string Code = "GebouwBevatGehistoreerdeGebouweenheden";
                public const string Message = "Deze actie is niet toegestaan wanneer er gehistoreerde gebouweenheden aanwezig zijn.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
