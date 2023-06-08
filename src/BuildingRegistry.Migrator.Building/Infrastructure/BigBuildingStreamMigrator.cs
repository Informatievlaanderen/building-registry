namespace BuildingRegistry.Migrator.Building.Infrastructure;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class BigBuildingStreamMigrator : StreamMigrator
{
    private readonly ProcessedIdsTable _processedIdsTable;

    public BigBuildingStreamMigrator(
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        ILifetimeScope lifetimeScope,
        Dictionary<Guid, int> consumedAddressItems)
        : base(
            loggerFactory,
            configuration,
            lifetimeScope,
            consumedAddressItems,
            new SqlBigStreamsTable(configuration.GetConnectionString("events")),
            loggerFactory.CreateLogger<BigBuildingStreamMigrator>(),
            "ProcessedIdsBigBuildings")
    {
        _processedIdsTable = new ProcessedIdsTable(configuration.GetConnectionString("events"), "ProcessedIds", loggerFactory);
    }

    protected override Task<bool> IsMigrated(int idInternal)
    {

        return _processedIdsTable.IsMigrated(idInternal);
    }
}
