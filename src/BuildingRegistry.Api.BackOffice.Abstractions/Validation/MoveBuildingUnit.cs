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
                public const string Code = "GebouwGehistoreerdOfNietGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met status 'gepland', 'inAanbouw' of 'gerealiseerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class SourceAndDestinationBuildingAreTheSame
            {
                public const string Code = "BrongebouwIdHetzelfdeAlsDoelgebouwId";
                public static string Message(string doelgebouwId) => $"Het brongebouwId is hetzelfde als het doelgebouwId: {doelgebouwId}.";

                public static TicketError ToTicketError(string doelgebouwId) => new TicketError(Message(doelgebouwId), Code);
            }
        }
    }
}
