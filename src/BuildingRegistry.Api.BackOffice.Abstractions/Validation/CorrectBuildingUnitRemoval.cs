namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectBuildingUnitRemoval
        {
            public static class InvalidBuildingStatus
            {
                public const string Code = "InvalidBuildingStatusCode";
                public const string Message = "InvalidBuildingStatusMessage";
            }
        }
    }
}
