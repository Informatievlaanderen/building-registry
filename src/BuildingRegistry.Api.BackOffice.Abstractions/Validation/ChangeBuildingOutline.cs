namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class ChangeBuildingOutline
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwGehistoreerdOfNietGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan op gebouwen met status 'gepland', 'inAanbouw' of 'gerealiseerd'.";

                public static TicketError ToTicketError() => new(Message, Code);
            }

            public static class BuildingHasBuildingUnitsOutsideChangedGeometry
            {
                public const string Code = "GebouweenheidGeomtrieBuitenGebouwGeometrie";
                public const string Message = "Het gebouw heeft onderliggende gebouweenheden met status 'gepland' of 'gerealiseerd' buiten de nieuw geschetste gebouwgeometrie.";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
