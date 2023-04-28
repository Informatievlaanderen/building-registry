namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class MeasureBuilding
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwGehistoreerdOfNietGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met status 'gepland', 'inAanbouw' of 'gerealiseerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
