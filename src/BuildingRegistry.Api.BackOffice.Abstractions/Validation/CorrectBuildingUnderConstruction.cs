namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class CorrectBuildingUnderConstruction
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwGerealiseerdGehistoreerdOfNietGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met status 'inAanbouw'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
