namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class RealizeBuildingUnit
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwStatusNietInGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan binnen een gerealiseerd gebouw.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingUnitInvalidStatus
            {
                public const string Code = "GebouweenheidGehistoreerdOfNietGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
