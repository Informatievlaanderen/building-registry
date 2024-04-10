using TicketingService.Abstractions;

namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class MoveBuildingUnit
        {
            public static class BuildingNotFound
            {
                public const string Code = "GebouwIdNietGekendValidatie";
                public static string MessageWithPuri(string puri) => $"Het gebouwId '{puri}' is niet gekend in het gebouwenregister.";
                public static TicketError ToTicketError(string puri) => new TicketError(MessageWithPuri(puri), Code);
            }

            public static class BuildingInvalidStatus
            {
                public const string Code = "TODO-rik";
                public const string Message = "Deze actie is enkel toegestaan binnen een gepland, inAanbouw of gerealiseerd gebouw.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingUnitInvalidStatus
            {
                public const string Code = "TODO-rik";
                public const string Message = "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland' of 'gerealiseerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
