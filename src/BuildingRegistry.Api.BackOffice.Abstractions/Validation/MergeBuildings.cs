namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class MergeBuildings
        {
            public static class BuildingNotFound
            {
                public const string Code = "GebouwIdNietGekendValidatie";
                public static string MessageWithPuri(string puri) => $"Het gebouwId '{puri}' is niet gekend in het gebouwenregister.";
                public static TicketError ToTicketError(string puri) => new TicketError(MessageWithPuri(puri), Code);
            }

            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwStatusGeplandInaanbouwNietgerealiseerdGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met status 'gerealiseerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class TooFewBuildings
            {
                public const string Code = "TooFewBuildings";
                public const string Message = "TooFewBuildings";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class TooManyBuildings
            {
                public const string Code = "TooManyBuildings";
                public const string Message = "TooManyBuildings";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
