namespace BuildingRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class CommonGrb
        {
            public static class Idempotency
            {
                public const string Code = "Idempotency";
                public static TicketError ToTicketError() => new(string.Empty, Code);
            }
        }
    }
}
