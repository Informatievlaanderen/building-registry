namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class RetireBuildingUnit
        {
            public static class BuildingInvalidStatus
            {
                public const string Code = "GebouwStatusNietInGeplandInAanbouwOfGerealiseerd";
                public const string Message = "Deze actie is enkel toegestaan binnen een gepland, inAanbouw of gerealiseerd gebouw.";
            }
        }
    }
}
