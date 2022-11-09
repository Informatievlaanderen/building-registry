namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectBuildingUnitRetirement
        {
            public static class InvalidStatus
            {
                public const string Code = "GebouweenheidNietGerealiseerdOfGepland";
                public const string Message = "Deze actie is enkel toegestaan op gebouweenheden met status 'gehistoreerd'.";
            }

            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwStatusNietInGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan binnen een gerealiseerd gebouw.";
            }
        }
    }
}
