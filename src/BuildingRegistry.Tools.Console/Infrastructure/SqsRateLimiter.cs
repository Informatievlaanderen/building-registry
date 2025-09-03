namespace BuildingRegistry.Tools.Console.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using NodaTime;

    public sealed class SqsRateLimiter<TSqsHandler, TSqsRequest>
        where TSqsHandler : SqsHandler<TSqsRequest>
        where TSqsRequest : SqsRequest
    {
        private const int ReassessIntervalMinutes = 2;

        private readonly TSqsHandler _sqsHandler;
        private readonly IClock _clock;
        private readonly SqsRateLimiterConfig _config;

        public SqsRateLimiter(
            TSqsHandler sqsHandler,
            IClock clock,
            SqsRateLimiterConfig config)
        {
            _sqsHandler = sqsHandler;
            _clock = clock;
            _config = config;
        }

        public async Task Handle<TId>(
            IList<TId> listToProcess,
            Func<TId, TSqsRequest> requestFactory,
            Func<TId, Task> actionAfterHandle,
            CancellationToken cancellationToken)
        {
            var rateLimit = GetCurrentRateLimit();
            var delayMs = 1000 / rateLimit;
            var lastReassess = _clock.GetCurrentInstant();

            foreach (var idToProcess in listToProcess)
            {
                var now = _clock.GetCurrentInstant();
                if ((now - lastReassess).TotalMinutes >= ReassessIntervalMinutes)
                {
                    rateLimit = GetCurrentRateLimit();
                    delayMs = 1000 / rateLimit;
                    lastReassess = now;
                }

                await _sqsHandler.Handle(requestFactory(idToProcess), cancellationToken);
                await actionAfterHandle.Invoke(idToProcess);
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        private int GetCurrentRateLimit()
        {
            var now = _clock.GetCurrentInstant().InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault());
            var hour = now.Hour;
            var isOfficeHours = hour >= _config.OfficeHoursStart && hour < _config.OfficeHoursEnd;
            var isWeekend = now.DayOfWeek == IsoDayOfWeek.Saturday || now.DayOfWeek == IsoDayOfWeek.Sunday;
            return isOfficeHours && !isWeekend ? _config.MaxRequestsPerSecondOfficeHours : _config.MaxRequestsPerSecondOutsideOfficeHours;
        }
    }

    public sealed class SqsRateLimiterConfig
    {
        public int OfficeHoursStart { get; set; }
        public int OfficeHoursEnd { get; set; }
        public int MaxRequestsPerSecondOfficeHours { get; set; }
        public int MaxRequestsPerSecondOutsideOfficeHours { get; set; }
    }
}
