namespace BuildingRegistry.Migrator.Building.Infrastructure;

using System;
using System.Collections.Generic;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class SmallBuildingStreamMigrator : StreamMigrator
{
    public SmallBuildingStreamMigrator(
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        ILifetimeScope lifetimeScope,
        Dictionary<Guid, int> consumedAddressItems)
        : base(
            loggerFactory,
            configuration,
            lifetimeScope,
            consumedAddressItems,
            new SqlSmallStreamsTable(configuration.GetConnectionString("events")),
            loggerFactory.CreateLogger<SmallBuildingStreamMigrator>(),
            "ProcessedIds")
    { }
}
