namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class MergeBuildings
        {
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
