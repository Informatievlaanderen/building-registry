namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class NotRealizeBuilding
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwGehistoreerdOfGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met status 'gepland' of 'inAanbouw'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingUnitInvalidStatus
            {
                public const string Code = "GebouweenheidGehistoreerdOfGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
