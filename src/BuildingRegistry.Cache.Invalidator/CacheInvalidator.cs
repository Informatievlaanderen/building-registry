namespace BuildingRegistry.Cache.Invalidator
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Building;
    using Consumer.Read.Parcel;
    using Microsoft.Extensions.Hosting;

    public class CacheInvalidator : BackgroundService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ConsumerParcelContext _consumerParcelContext;
        private readonly IRedisCacheInvalidateService _redisCacheInvalidateService;

        public CacheInvalidator(
            IHostApplicationLifetime hostApplicationLifetime,
            ConsumerParcelContext consumerParcelContext,
            IRedisCacheInvalidateService redisCacheInvalidateService)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _consumerParcelContext = consumerParcelContext;
            _redisCacheInvalidateService = redisCacheInvalidateService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var buildingsToInvalidate = _consumerParcelContext.BuildingsToInvalidate.ToList();

            var buildingPersistentLocalIds = buildingsToInvalidate
                .Select(x => new BuildingPersistentLocalId(x.BuildingPersistentLocalId))
                .ToList();
            await _redisCacheInvalidateService.Invalidate(buildingPersistentLocalIds);

            _consumerParcelContext.BuildingsToInvalidate.RemoveRange(buildingsToInvalidate);
            await _consumerParcelContext.SaveChangesAsync(stoppingToken);

            _hostApplicationLifetime.StopApplication();
        }
    }
}
