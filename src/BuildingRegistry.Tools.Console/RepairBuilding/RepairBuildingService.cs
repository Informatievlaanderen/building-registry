namespace BuildingRegistry.Tools.Console.RepairBuilding
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions.Building.SqsRequests;
    using Be.Vlaanderen.Basisregisters.GrAr.Notifications;
    using Infrastructure;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using TicketingService.Abstractions;

    public class RepairBuildingService : BackgroundService
    {
        private readonly SqsRateLimiter<RepairBuildingSqsHandler, RepairBuildingSqsRequest> _sqsRateLimiter;
        private readonly RepairBuildingRepository _repairBuildingRepository;
        private readonly ITicketing _ticketing;
        private readonly ProjectionRepository _projectionRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<RepairBuildingService> _logger;

        public RepairBuildingService(
            SqsRateLimiter<RepairBuildingSqsHandler, RepairBuildingSqsRequest> sqsRateLimiter,
            RepairBuildingRepository repairBuildingRepository,
            ITicketing ticketing,
            ProjectionRepository projectionRepository,
            INotificationService notificationService,
            ILoggerFactory loggerFactory)
        {
            _sqsRateLimiter = sqsRateLimiter;
            _repairBuildingRepository = repairBuildingRepository;
            _ticketing = ticketing;
            _projectionRepository = projectionRepository;
            _notificationService = notificationService;
            _logger = loggerFactory.CreateLogger<RepairBuildingService>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _repairBuildingRepository.EnsureSchemaAndTablesExist();
            await _repairBuildingRepository.FillBuildingToProcess();
            var idsToProcess = (await _repairBuildingRepository.GetBuildingsToProcess()).ToList();
            _logger.LogInformation("Found {idsToProcess} buildings to process.", idsToProcess.Count);

            _ = ScheduleBuildingRepairUpdates(stoppingToken);
            var ticketId = await _ticketing.CreateTicket(null, stoppingToken);

            await _sqsRateLimiter.Handle<int>(
                idsToProcess,
                id => new RepairBuildingSqsRequest { BuildingPersistentLocalId = id, TicketId = ticketId },
                async processedId =>
                {
                    await _repairBuildingRepository.DeleteBuilding(processedId);
                    _logger.LogInformation("Processed building {processedId}", processedId);

                    if (processedId % 100 == 0)
                    {
                        await WaitIfProducerProjectionBehindAsync(stoppingToken);
                    }
                },
                stoppingToken);

            _logger.LogInformation("Building repair processing completed.");
        }

        private async Task WaitIfProducerProjectionBehindAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                var producerPosition = await _projectionRepository.GetProducerPosition();
                var headPosition = await _projectionRepository.GetMaxAllStreamPosition();

                var lag = headPosition - producerPosition;
                if (lag <= 100)
                {
                    _logger.LogInformation("Resume processing, lag is {Lag} messages.", lag);
                    break;
                }

                _logger.LogInformation("Producer projection is behind by {Lag} messages, waiting 3 seconds...", lag);
                await Task.Delay(3000, cancellationToken);
            }
        }

        private async Task ScheduleBuildingRepairUpdates(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromHours(12);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var count = await _repairBuildingRepository.GetCount();
                    var message = $"RepairBuildingService: {count} buildings left to process.";
                    await _notificationService.PublishToTopicAsync(new NotificationMessage("BuildingTools", message, "BuildingTools", NotificationSeverity.Good));
                    _logger.LogInformation(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending notification.");
                }

                await Task.Delay(interval, stoppingToken);
            }
        }
    }
}
