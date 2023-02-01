namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class PlanBuildingUnit
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouweenheidGebouwIdNietGerealiseerdofGehistoreerd";
                public const string Message = "De gebouwId is niet gerealiseerd of gehistoreerd.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingUnitPositionOutsideBuildingGeometry
            {
                public const string Code = "GebouweenheidOngeldigePositieValidatie";
                public const string Message = "De positie dient binnen de geometrie van het gebouw te liggen.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
