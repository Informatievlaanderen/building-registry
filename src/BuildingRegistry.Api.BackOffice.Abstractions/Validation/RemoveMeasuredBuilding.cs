namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class RemoveMeasuredBuilding
        {
            public static class BuildingHasInvalidBuildingGeometryMethod
            {
                public const string Code = "GebouwGeometrieMethodeIngeschetst";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met geometrieMethode 'ingemeten'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingHasBuildingUnits
            {
                public const string Code = "GebouwHeeftGebouweenheden";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen zonder gebouweenheden.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
