namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class Common
        {
            public static class CommonBuildingUnit
            {
                public static class InvalidFunction
                {
                    public const string Code = "GebouweenheidGemeenschappelijkDeel";
                    public const string Message = "Deze actie is niet toegestaan op gebouweenheden met functie gemeenschappelijkDeel.";

                    public static TicketError ToTicketError() => new(Message, Code);
                }
            }

            public static class AdresIdInvalid
            {
                public const string Code = "AdresOngeldig";
                public const string Message = "Ongeldig AdresId.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
