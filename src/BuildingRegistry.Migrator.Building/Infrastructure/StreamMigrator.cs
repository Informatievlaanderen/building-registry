namespace BuildingRegistry.Migrator.Building.Infrastructure
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using BuildingRegistry.Building;
    using Legacy;
    using Legacy.Commands;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Serilog;
    using BuildingGeometry = Legacy.BuildingGeometry;
    using BuildingUnit = BuildingRegistry.Building.Commands.BuildingUnit;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    public abstract class StreamMigrator
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILogger _logger;

        private readonly SqlStreamsTable _sqlStreamsTable;
        private readonly ProcessedIdsTable _processedIdsTable;
        private readonly Dictionary<Guid, int> _consumedAddressItems;
        private readonly bool _skipIncomplete;
        private readonly List<Guid> _buildingsAllowedToBeSkipped;

        private readonly Stopwatch _stopwatch = new Stopwatch();

        private List<(int processedId, bool isPageCompleted)> _processedIds = new List<(int processedId, bool isPageCompleted)>();

        protected StreamMigrator(
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            ILifetimeScope lifetimeScope,
            Dictionary<Guid, int> consumedAddressItems,
            SqlStreamsTable streamsTable,
            ILogger logger,
            string processedIdsTableName)
        {
            _lifetimeScope = lifetimeScope;
            _consumedAddressItems = consumedAddressItems;

            var connectionString = configuration.GetConnectionString("events");
            _processedIdsTable = new ProcessedIdsTable(connectionString, processedIdsTableName, loggerFactory);

            _logger = logger;
            _sqlStreamsTable = streamsTable;

            _skipIncomplete = bool.Parse(configuration["SkipIncomplete"]);
            _buildingsAllowedToBeSkipped = configuration["BuildingsAllowedToBeSkipped"]
                .Split(',')
                .Select(Guid.Parse)
                .ToList();
        }

        public async Task ProcessAsync(CancellationToken ct)
        {
            await _processedIdsTable.CreateTableIfNotExists();

            var processedIdsList = await _processedIdsTable.GetProcessedIds();
            _processedIds = new List<(int, bool)>(processedIdsList);

            var lastCursorPosition = _processedIds.Any()
                ? _processedIds
                    .Where(x => x.isPageCompleted)
                    .Select(x => x.processedId)
                    .DefaultIfEmpty(0)
                    .Max()
                : 0;

            var pageOfStreams = (await _sqlStreamsTable.ReadNextStreamPage(lastCursorPosition)).ToList();

            while (pageOfStreams.Any() && !ct.IsCancellationRequested)
            {
                try
                {
                    var processedPageItems = await ProcessStreams(pageOfStreams, ct);

                    if (!processedPageItems.Any())
                    {
                        lastCursorPosition = pageOfStreams.Max(x => x.internalId);
                    }
                    else
                    {
                        await _processedIdsTable.CompletePageAsync(pageOfStreams.Select(x => x.internalId).ToList());
                        processedPageItems.ForEach(x => _processedIds.Add((x, true)));
                        lastCursorPosition = _processedIds.Max(x => x.processedId);
                    }

                    pageOfStreams = (await _sqlStreamsTable.ReadNextStreamPage(lastCursorPosition)).ToList();
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("ProcessStreams cancelled.");
                }
            }
        }

        private async Task<List<int>> ProcessStreams(IEnumerable<(int, string)> streamsToProcess, CancellationToken ct)
        {
            var processedItems = new ConcurrentBag<int>();

            await Parallel.ForEachAsync(streamsToProcess, ct, async (stream, innerCt) =>
            {
                try
                {
                    await Policy
                        .Handle<SqlException>()
                        .WaitAndRetryAsync(10,
                            currentRetry => Math.Pow(currentRetry, 2) * TimeSpan.FromSeconds(30),
                            (_, timespan) =>
                                Log.Information($"SqlException occurred retrying after {timespan.Seconds} seconds."))
                        .ExecuteAsync(async () =>
                        {
                            await ProcessStream(stream, processedItems, innerCt);
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(
                        $"Unexpected exception for migration stream '{stream.Item1}', aggregateId '{stream.Item2}' \n\n {ex.Message}");
                    throw;
                }
            });

            return processedItems.ToList();
        }


        protected virtual Task<bool> IsMigrated(int idInternal)
        {
            return Task.FromResult(false);
        }

        private async Task ProcessStream(
            (int, string) stream,
            ConcurrentBag<int> processedItems,
            CancellationToken ct)
        {
            var (idInternal, aggregateId) = stream;

            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (_processedIds.Contains((idInternal, false)))
            {
                _logger.LogDebug($"Already migrated '{idInternal}', skipping...");
                return;
            }

            if (await IsMigrated(idInternal))
            {
                _logger.LogDebug($"Building Already migrated in Small buildings '{idInternal}', skipping...");
                return;
            }

            await using var streamLifetimeScope = _lifetimeScope.BeginLifetimeScope();

            var legacyBuildingsRepo = streamLifetimeScope.Resolve<Legacy.IBuildings>();
            var buildingId = new Legacy.BuildingId(Guid.Parse(aggregateId));

            _stopwatch.Start();
            var legacyBuildingAggregate = await legacyBuildingsRepo.GetAsync(buildingId, ct);
            _stopwatch.Stop();
            _logger.LogInformation("Resolved aggregate in {timing}", _stopwatch.Elapsed.ToString("g", CultureInfo.InvariantCulture));
            _stopwatch.Reset();

            if (!legacyBuildingAggregate.IsComplete)
            {
                if (legacyBuildingAggregate.IsRemoved)
                {
                    _logger.LogDebug($"Skipping incomplete & removed Building '{aggregateId}'.");
                    return;
                }

                if(_buildingsAllowedToBeSkipped.Contains(buildingId))
                {
                    _logger.LogDebug($"Skipping incomplete Building '{aggregateId}'.");
                    return;
                }

                if (_skipIncomplete)
                {
                    return;
                }

                throw new InvalidOperationException($"Incomplete but not removed Building '{aggregateId}'.");
            }

            List<BuildingUnit> BuildingUnitMapper(List<Legacy.BuildingUnit> legacyBuildingUnits, BuildingGeometry legacyBuildingGeometry)
            {
                var commandBuildingUnits = new List<BuildingUnit>();

                foreach (var legacyBuildingUnit in legacyBuildingUnits)
                {
                    if (!legacyBuildingUnit.IsComplete || legacyBuildingUnit.IsRemoved)
                    {
                        if (legacyBuildingUnit.IsRemoved)
                        {
                            _logger.LogDebug($"Skipping incomplete & removed BuildingUnit '{legacyBuildingUnit.PersistentLocalId}'.");
                            continue;
                        }

                        legacyBuildingUnit.Complete();

                        //throw new InvalidOperationException($"Incomplete but not removed BuildingUnit '{legacyBuildingUnit.PersistentLocalId}'.");
                    }

                    var status = legacyBuildingUnit.Status ?? throw new InvalidOperationException($"No status found for BuildingUnit '{legacyBuildingUnit.PersistentLocalId}'");

                    var addressPersistentLocalIds = new List<AddressPersistentLocalId>();
                    foreach (var addressId in legacyBuildingUnit.AddressIds)
                    {
                        if (!_consumedAddressItems.TryGetValue(addressId, out var addressPersistentLocalId))
                        {
                            //removed OR incomplete OR incorrect status
                            _logger.LogWarning($"Not migrating address '{addressId}' because it was not found in the AddressDetails table.");
                            continue;
                        }

                        addressPersistentLocalIds.Add(new AddressPersistentLocalId(addressPersistentLocalId));
                    }

                    commandBuildingUnits.Add(new BuildingUnit(legacyBuildingUnit.BuildingUnitId, legacyBuildingUnit.PersistentLocalId, legacyBuildingUnit.Function, status, addressPersistentLocalIds, legacyBuildingUnit.BuildingUnitPosition, legacyBuildingGeometry, legacyBuildingUnit.IsRemoved));
                }

                return commandBuildingUnits;
            }

            var migrateBuilding = legacyBuildingAggregate.CreateMigrateCommand(BuildingUnitMapper);
            var markMigrated = new MarkBuildingAsMigrated(
                new Legacy.BuildingId(migrateBuilding.BuildingId),
                new PersistentLocalId(migrateBuilding.BuildingPersistentLocalId),
                migrateBuilding.Provenance);

            await DispatchCommand(markMigrated, ct);
            await DispatchCommand(migrateBuilding, ct);

            await _processedIdsTable.Add(idInternal);
            processedItems.Add(idInternal);

            await using var backOfficeContext = streamLifetimeScope.Resolve<BackOfficeContext>();
            foreach (var buildingUnit in migrateBuilding.BuildingUnits)
            {
                await backOfficeContext
                    .BuildingUnitBuildings.AddAsync(
                        new BuildingUnitBuilding(
                            buildingUnit.BuildingUnitPersistentLocalId,
                            migrateBuilding.BuildingPersistentLocalId), ct);
            }
            await backOfficeContext.SaveChangesAsync(ct);
        }

        private async Task DispatchCommand<TCommand>(
            TCommand command,
            CancellationToken ct)
        where TCommand : IHasCommandProvenance
        {
            await using var scope = _lifetimeScope.BeginLifetimeScope();
            var cmdResolver = scope.Resolve<ICommandHandlerResolver>();
            await cmdResolver.Dispatch(
                command.CreateCommandId(),
                command,
                cancellationToken: ct);
        }
    }
}
