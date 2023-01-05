using TicketingService.Abstractions;

namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class AttachAddressToBuildingUnit
        {
            public static class BuildingUnitInvalidStatus
            {
                public const string Code = "GebouweenheidNietGerealiseerdOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland' of 'gerealiseerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class AddressInvalidStatus
            {
                public const string Code = "GebouweenheidAdresAfgekeurdOfGehistoreerd";
                public const string Message = "Het adres is afgekeurd of gehistoreerd.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
