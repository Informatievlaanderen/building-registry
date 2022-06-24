namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs.Building
{
    internal static class SqsQueueName
    {
        public const string Value = $"{nameof(BuildingRegistry)}.{nameof(Api)}.{nameof(BackOffice)}.{nameof(Building)}";
    }
}
