namespace BuildingRegistry.Producer.Ldes
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Notifications;
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo.SnapshotProducer;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using NodaTime;

    public abstract class SnapshotReproducer : BackgroundService
    {
        private readonly IOsloProxy _osloProxy;
        private readonly IProducer _producer;
        private readonly IClock _clock;
        private readonly INotificationService _notificationService;
        private readonly int _utcHourToRunWithin;
        private readonly ILogger<SnapshotReproducer> _logger;

        public SnapshotReproducer(
            IOsloProxy osloProxy,
            IProducer producer,
            IClock clock,
            INotificationService notificationService,
            int utcHourToRunWithin,
            ILoggerFactory loggerFactory)
        {
            _osloProxy = osloProxy;
            _producer = producer;
            _notificationService = notificationService;
            _utcHourToRunWithin = utcHourToRunWithin;
            _clock = clock;

            _logger = loggerFactory.CreateLogger<SnapshotReproducer>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = _clock.GetCurrentInstant().ToDateTimeUtc();
                if (now.Hour == _utcHourToRunWithin)
                {
                    _logger.LogInformation($"Starting {GetType().Name}");

                    try
                    {
                        //execute query
                        var idsToProcess = GetIdsToProcess(now);

                        //reproduce
                        foreach (var id in idsToProcess)
                        {
                            try
                            {
                                await FindAndProduce(async () =>
                                        await _osloProxy.GetSnapshot(id.PersistentLocalId.ToString(), stoppingToken),
                                    id.Position,
                                    stoppingToken);
                            }
                            catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.Gone)
                            {
                                _logger.LogInformation($"Snapshot '{id}' gone");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error while reproducing snapshot {id}", ex);
                                throw;
                            }
                        }

                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);

                        await _notificationService.PublishToTopicAsync(new NotificationMessage(
                            GetType().Name,
                            $"Reproducing snapshot failed: {ex}",
                            GetType().Name,
                            NotificationSeverity.Danger));
                    }
                }

                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }

        protected async Task FindAndProduce(Func<Task<OsloResult?>> findMatchingSnapshot, long storePosition, CancellationToken ct)
        {
            var result = await findMatchingSnapshot.Invoke();

            if (result != null)
            {
                await Produce(result.Identificator.Id, result.Identificator.ObjectId, result.JsonContent, storePosition, ct);
            }
        }

        protected async Task Produce(string puri, string objectId, string jsonContent, long storePosition, CancellationToken cancellationToken = default)
        {
            var result = await _producer.Produce(
                new MessageKey(puri),
                jsonContent,
                new List<MessageHeader> { new MessageHeader(MessageHeader.IdempotenceKey, $"{objectId}-{storePosition.ToString()}") },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }

        protected abstract List<(int PersistentLocalId, long Position)> GetIdsToProcess(DateTime utcNow);
    }
}
