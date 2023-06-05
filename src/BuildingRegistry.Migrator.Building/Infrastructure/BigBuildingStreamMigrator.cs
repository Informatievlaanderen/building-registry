namespace BuildingRegistry.Migrator.Building.Infrastructure;

using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class BigBuildingStreamMigrator : StreamMigrator
{
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
    { }
}
