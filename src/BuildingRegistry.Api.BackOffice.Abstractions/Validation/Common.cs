namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class Common
        {
            public static class CommonBuildingUnit
            {
                public static class Forbidden
                {
                    public const string Code = "GebouweenheidGemeenschappelijkDeel";
                    public const string Message = "Deze actie is niet toegestaan op gebouweenheden met functie gemeenschappelijkDeel.";
                }
            }
        }
    }
}
