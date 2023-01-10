namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class RemoveBuilding
        {
            public static class BuildingHasInvalidBuildingGeometryMethod
            {
                public const string Code = "GebouwGeometrieMethodeIngemetenGRB";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met geometrieMethode 'ingeschetst'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
