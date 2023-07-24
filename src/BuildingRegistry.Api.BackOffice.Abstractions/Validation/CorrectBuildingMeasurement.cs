namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class CorrectBuildingMeasurement
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwGeplandInAanbouwGehistoreerdOfNietGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met status 'gerealiseerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingIsOutlined
            {
                public const string Code = "GebouwGeometrieGeschetst";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met geometriemethode 'ingemeten'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
