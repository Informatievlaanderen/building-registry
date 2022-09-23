namespace BuildingRegistry.Api.BackOffice.Handlers.Sqs
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;

    public interface ISqsQueue
    {
        Task<bool> Copy<T>(
            T message,
            SqsQueueOptions queueOptions,
            CancellationToken cancellationToken)
            where T : class;
    }
}
