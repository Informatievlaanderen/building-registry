namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class PlaceBuildingUnderConstruction
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwGehistoreerdGerealiseerdOfNietGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met status 'gepland'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
