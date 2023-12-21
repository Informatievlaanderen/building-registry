namespace BuildingRegistry.Projections.Integration
{
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Converters;
    using Infrastructure;
    using Microsoft.Extensions.Options;

    [ConnectedProjectionName("Integratie gebouweenheid latest item")]
    [ConnectedProjectionDescription("Projectie die de laatste gebouweenheid data voor de integratie database bijhoudt.")]
    public sealed class BuildingUnitLatestItemProjections : ConnectedProjection<IntegrationContext>
    {
        public BuildingUnitLatestItemProjections(IOptions<IntegrationOptions> options)
        {
            var wkbReader = WKBReaderFactory.Create();

            #region Building

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    var geometryAsBinary = buildingUnit.ExtendedWkbGeometry.ToByteArray();
                    var sysGeometry = wkbReader.Read(geometryAsBinary);

                    var buildingUnitLatestItem = new BuildingUnitLatestItem
                    {
                        BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                        BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                        Status = BuildingUnitStatus.Parse(buildingUnit.Status).Map(),
                        Function = BuildingUnitFunction.Parse(buildingUnit.Function).Map(),
                        GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                        Geometry = sysGeometry,
                        HasDeviation = false,
                        IsRemoved = message.Message.IsRemoved,
                        VersionTimestamp = message.Message.Provenance.Timestamp,
                        Namespace = options.Value.BuildingUnitNamespace,
                        PuriId = $"{options.Value.BuildingUnitNamespace}/{buildingUnit.BuildingUnitPersistentLocalId}",
                        IdempotenceKey = message.Position
                    };

                    await context
                        .BuildingUnitLatestItems
                        .AddAsync(buildingUnitLatestItem, ct);

                    var addressPersistentLocalIds = buildingUnit.AddressPersistentLocalIds.Distinct();
                    foreach (var addressPersistentLocalId in addressPersistentLocalIds)
                    {
                        await context.AddIdempotentBuildingUnitAddress(buildingUnitLatestItem, addressPersistentLocalId, ct);
                    }
                }
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                if (!message.Message.BuildingUnitPersistentLocalIds.Any())
                {
                    return;
                }

                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                {
                    await context.FindAndUpdateBuildingUnit(
                        buildingUnitPersistentLocalId,
                        message.Position,
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                            UpdateVersionTimestamp(buildingUnit, message.Message);
                            return Task.CompletedTask;
                        },
                        ct);
                }
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                if (!message.Message.BuildingUnitPersistentLocalIds.Any()
                    && !message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived.Any())
                {
                    return;
                }

                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds
                             .Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await context.FindAndUpdateBuildingUnit(
                        buildingUnitPersistentLocalId,
                        message.Position,
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                            UpdateVersionTimestamp(buildingUnit, message.Message);
                            return Task.CompletedTask;
                        },
                        ct);
                }
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                if (!message.Message.BuildingUnitPersistentLocalIds.Any()
                    && !message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived.Any())
                {
                    return;
                }

                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds
                             .Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await context.FindAndUpdateBuildingUnit(
                        buildingUnitPersistentLocalId,
                        message.Position,
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                            UpdateVersionTimestamp(buildingUnit, message.Message);
                            return Task.CompletedTask;
                        },
                        ct);
                }
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                if (!message.Message.BuildingUnitPersistentLocalIds.Any()
                    && !message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived.Any())
                {
                    return;
                }

                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds
                             .Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                {
                    await context.FindAndUpdateBuildingUnit(
                        buildingUnitPersistentLocalId,
                        message.Position,
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                            UpdateVersionTimestamp(buildingUnit, message.Message);
                            return Task.CompletedTask;
                        },
                        ct);
                }
            });

            #endregion

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var buildingUnitLatestItem = new BuildingUnitLatestItem
                {
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Status = BuildingUnitStatus.Planned.Map(),
                    Function = BuildingUnitFunction.Parse(message.Message.Function).Map(),
                    GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                    Geometry = sysGeometry,
                    HasDeviation = message.Message.HasDeviation,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.BuildingUnitNamespace,
                    PuriId = $"{options.Value.BuildingUnitNamespace}/{message.Message.BuildingUnitPersistentLocalId}",
                    IdempotenceKey = message.Position
                };

                await context
                    .BuildingUnitLatestItems
                    .AddAsync(buildingUnitLatestItem, ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized.Map();
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized.Map();
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned.Map();
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned.Map();
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned.Map();
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Retired.Map();
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized.Map();
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.IsRemoved = true;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.IsRemoved = true;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnit.Status = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Map();
                        buildingUnit.Function = BuildingUnitFunction.Parse(message.Message.Function).Map();
                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                        buildingUnit.HasDeviation = message.Message.HasDeviation;
                        buildingUnit.IsRemoved = false;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = false;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = true;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = true;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = false;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var buildingUnitLatestItem = new BuildingUnitLatestItem
                {
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Status =  BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Map(),
                    Function = BuildingUnitFunction.Common.Map(),
                    GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                    Geometry = sysGeometry,
                    HasDeviation = message.Message.HasDeviation,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.BuildingUnitNamespace,
                    PuriId = $"{options.Value.BuildingUnitNamespace}/{message.Message.BuildingUnitPersistentLocalId}",
                    IdempotenceKey = message.Position
                };

                await context
                    .BuildingUnitLatestItems
                    .AddAsync(buildingUnitLatestItem, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    async buildingUnit =>
                    {
                        await context.AddIdempotentBuildingUnitAddress(buildingUnit, message.Message.AddressPersistentLocalId, ct);
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    async buildingUnit =>
                    {
                        await context.RemoveIdempotentBuildingUnitAddress(buildingUnit, message.Message.AddressPersistentLocalId, ct);
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    async buildingUnit =>
                    {
                        await context.RemoveIdempotentBuildingUnitAddress(buildingUnit, message.Message.AddressPersistentLocalId, ct);
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    async buildingUnit =>
                    {
                        await context.RemoveIdempotentBuildingUnitAddress(buildingUnit, message.Message.AddressPersistentLocalId, ct);
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    async buildingUnit =>
                    {
                        await context.RemoveIdempotentBuildingUnitAddress(buildingUnit, message.Message.AddressPersistentLocalId, ct);
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    async buildingUnit =>
                    {
                        await context.RemoveIdempotentBuildingUnitAddress(buildingUnit, message.Message.PreviousAddressPersistentLocalId, ct);
                        await context.AddIdempotentBuildingUnitAddress(buildingUnit, message.Message.NewAddressPersistentLocalId, ct);
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Retired.Map();
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasTransferred>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    message.Position,
                    async buildingUnit =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnit.BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId;
                        buildingUnit.Status = BuildingUnitStatus.Parse(message.Message.Status).Map();
                        buildingUnit.Function = BuildingUnitFunction.Parse(message.Message.Function).Map();
                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                        buildingUnit.HasDeviation = message.Message.HasDeviation;
                        UpdateVersionTimestamp(buildingUnit, message.Message);

                        var addressPersistentLocalIds = message.Message.AddressPersistentLocalIds.Distinct();
                        foreach (var addressPersistentLocalId in addressPersistentLocalIds)
                        {
                            await context.AddIdempotentBuildingUnitAddress(buildingUnit, addressPersistentLocalId, ct);
                        }
                    },
                    ct);
            });

            // BuildingUnitWasTransferred couples the unit to another building and BuildingUnitMoved is an event applicable on the old building.
            When<Envelope<BuildingUnitWasMoved>>((_, _, _) => Task.CompletedTask);
        }

        private static void UpdateVersionTimestamp(BuildingUnitLatestItem building, IHasProvenance message)
            => building.VersionTimestamp = message.Provenance.Timestamp;
    }
}
