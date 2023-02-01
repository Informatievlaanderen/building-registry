namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class NotRealizeBuildingUnit
        {
            public static class BuildingUnitInvalidStatus
            {
                public const string Code = "GebouweenheidGehistoreerdOfGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouweenheden met status 'gepland'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
