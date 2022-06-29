namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs
{
    internal static class SqsQueueName
    {
        public const string Value = $"{nameof(BuildingRegistry)}.{nameof(Api)}.{nameof(BackOffice)}";
    }
}
