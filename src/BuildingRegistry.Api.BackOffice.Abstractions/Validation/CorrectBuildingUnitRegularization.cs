using TicketingService.Abstractions;

namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectBuildingUnitRegularization
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
        }
    }
}
