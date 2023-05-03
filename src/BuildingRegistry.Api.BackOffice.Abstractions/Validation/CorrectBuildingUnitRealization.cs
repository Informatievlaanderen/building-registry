using TicketingService.Abstractions;

namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectBuildingUnitRealization
        {
            public static class BuildingUnitInvalidStatus
            {
                public const string Code = "GebouweenheidGehistoreerdNietGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouweenheden met status 'gerealiseerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

        }
    }
}
