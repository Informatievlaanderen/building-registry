namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class CorrectBuildingUnitRemoval
        {
            public static class InvalidBuildingUnitStatus
            {
                public const string Code = "InvalidBuildingUnitStatusCode";
                public const string Message = "InvalidBuildingUnitStatusMessage";
            }

            public static class InvalidBuildingStatus
            {
                public const string Code = "InvalidBuildingStatusCode";
                public const string Message = "InvalidBuildingStatusMessage";
            }
        }
    }
}
