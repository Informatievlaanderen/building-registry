namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class DemolishBuilding
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwStatusGeplandInaanbouwNietgerealiseerdGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met status 'gerealiseerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class InvalidGeometryMethod
            {
                public const string Code = "GebouwGeometrieMethodeGeschetst";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met geometrieMethode 'ingemetenGRB'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
