using TicketingService.Abstractions;

namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectBuildingNotRealization
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwInAanbouwGerealiseerdOfGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op geschetste gebouwen met status 'nietGerealiseerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
