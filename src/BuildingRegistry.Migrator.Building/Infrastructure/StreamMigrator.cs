namespace BuildingRegistry.Migrator.Building.Infrastructure
{
    using System;
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
    using Consumer.Address;
    using Legacy;
    using Legacy.Commands;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Serilog;
    using BuildingUnit = BuildingRegistry.Building.Commands.BuildingUnit;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    internal class StreamMigrator
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ILogger _logger;
        private readonly ProcessedIdsTable _processedIdsTable;
        private readonly SqlStreamsTable _sqlStreamTable;
        private Dictionary<Guid, int> _consumedAddressItems;
        private readonly bool _skipIncomplete;

        private List<(int processedId, bool isPageCompleted)> _processedIds;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public StreamMigrator(ILoggerFactory loggerFactory, IConfiguration configuration, ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
            _logger = loggerFactory.CreateLogger("BuildingMigrator");

            var connectionString = configuration.GetConnectionString("events");
            _processedIdsTable = new ProcessedIdsTable(connectionString, loggerFactory);
            _sqlStreamTable = new SqlStreamsTable(connectionString);

            _skipIncomplete = bool.Parse(configuration["SkipIncomplete"]);
        }

        public async Task ProcessAsync(CancellationToken ct)
        {
            await _processedIdsTable.CreateTableIfNotExists();

            var consumerContext = _lifetimeScope.Resolve<ConsumerAddressContext>();
            _consumedAddressItems = await consumerContext
                .AddressConsumerItems
                .Where(x => x.AddressId != null)
                .Select(x => new { AddressId = x.AddressId.Value, x.AddressPersistentLocalId })
                .ToDictionaryAsync(x => x.AddressId, y => y.AddressPersistentLocalId, ct);

            var processedIdsList = await _processedIdsTable.GetProcessedIds();
            _processedIds = new List<(int, bool)>(processedIdsList);

            var lastCursorPosition = _processedIds.Any()
                ? _processedIds
                    .Where(x => x.isPageCompleted)
                    .Select(x => x.processedId)
                    .DefaultIfEmpty(0)
                    .Max()
                : 0;

            var pageOfStreams = (await _sqlStreamTable.ReadNextBuildingStreamPage(lastCursorPosition)).ToList();

            while (pageOfStreams.Any())
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

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

                    pageOfStreams = (await _sqlStreamTable.ReadNextBuildingStreamPage(lastCursorPosition)).ToList();
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("ProcessStreams cancelled.");
                }
            }
        }

        private async Task<List<int>> ProcessStreams(IEnumerable<(int, string)> streamsToProcess, CancellationToken ct)
        {
            var processedItems = new List<int>();

            foreach (var stream in streamsToProcess)
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
                            await ProcessStream(stream, processedItems, ct);
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(
                        $"Unexpected exception for migration stream '{stream.Item1}', aggregateId '{stream.Item2}' \n\n {ex.Message}");
                    throw;
                }
            }

            return processedItems;
        }

        private async Task ProcessStream(
            (int, string) stream,
            List<int> processedItems,
            CancellationToken ct)
        {
            var (internalId, aggregateId) = stream;

            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (_processedIds.Contains((internalId, false)))
            {
                _logger.LogDebug($"Already migrated '{internalId}', skipping...");
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

                if (_skipIncomplete)
                {
                    return;
                }

                throw new InvalidOperationException($"Incomplete but not removed Building '{aggregateId}'.");
            }

            List<BuildingUnit> BuildingUnitMapper(List<Legacy.BuildingUnit> legacyBuildingUnits)
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

                        if (_skipIncomplete)
                        {
                            continue;
                        }

                        throw new InvalidOperationException($"Incomplete but not removed BuildingUnit '{legacyBuildingUnit.PersistentLocalId}'.");
                    }

                    var status = legacyBuildingUnit.Status ?? throw new InvalidOperationException($"No status found for BuildingUnit '{legacyBuildingUnit.PersistentLocalId}'");

                    var addressPersistentLocalIds = new List<AddressPersistentLocalId>();
                    foreach (var addressId in legacyBuildingUnit.AddressIds)
                    {
                        if (!_consumedAddressItems.TryGetValue(addressId, out var addressPersistentLocalId))
                        {
                            if (_skipIncomplete)
                            {
                                continue;
                            }

                            throw new InvalidOperationException($"AddressConsumerItem for addressId '{addressId}' was not found in the ConsumerAddressContext.");
                        }

                        addressPersistentLocalIds.Add(new AddressPersistentLocalId(addressPersistentLocalId));
                    }

                    commandBuildingUnits.Add(new BuildingUnit(legacyBuildingUnit.BuildingUnitId, legacyBuildingUnit.PersistentLocalId, legacyBuildingUnit.Function, status, addressPersistentLocalIds, legacyBuildingUnit.BuildingUnitPosition, legacyBuildingUnit.IsRemoved));
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

            await _processedIdsTable.Add(internalId);
            processedItems.Add(internalId);

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
            await using (var scope = _lifetimeScope.BeginLifetimeScope())
            {
                var cmdResolver = scope.Resolve<ICommandHandlerResolver>();
                await cmdResolver.Dispatch(
                    command.CreateCommandId(),
                    command,
                    cancellationToken: ct);
            }
        }
    }
}
