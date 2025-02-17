namespace BuildingRegistry.Projections.Integration.BuildingUnit.LatestItem
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Converters;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
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
                        OsloStatus = BuildingUnitStatus.Parse(buildingUnit.Status).Map(),
                        Status = buildingUnit.Status,
                        OsloFunction = BuildingUnitFunction.Parse(buildingUnit.Function).Map(),
                        Function = buildingUnit.Function,
                        OsloGeometryMethod = BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod).Map(),
                        GeometryMethod = buildingUnit.GeometryMethod,
                        Geometry = sysGeometry,
                        HasDeviation = false,
                        IsRemoved = buildingUnit.IsRemoved,
                        VersionTimestamp = message.Message.Provenance.Timestamp,
                        Namespace = options.Value.BuildingUnitNamespace,
                        Puri = $"{options.Value.BuildingUnitNamespace}/{buildingUnit.BuildingUnitPersistentLocalId}"
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
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod;
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
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod;
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
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod;
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
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod;
                            UpdateVersionTimestamp(buildingUnit, message.Message);
                            return Task.CompletedTask;
                        },
                        ct);
                }
            });

            When<Envelope<BuildingWasPlannedV2>>(DoNothing);
            When<Envelope<BuildingBecameUnderConstructionV2>>(DoNothing);
            When<Envelope<BuildingWasRealizedV2>>(DoNothing);
            When<Envelope<BuildingWasNotRealizedV2>>(DoNothing);
            When<Envelope<BuildingWasDemolished>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(DoNothing);
            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(DoNothing);
            When<Envelope<BuildingGeometryWasImportedFromGrb>>(DoNothing);
            When<Envelope<BuildingWasRemovedV2>>(DoNothing);
            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(DoNothing);

            #endregion

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var buildingUnitLatestItem = new BuildingUnitLatestItem
                {
                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    OsloStatus = BuildingUnitStatus.Planned.Map(),
                    Status = BuildingUnitStatus.Planned.Status,
                    OsloFunction = BuildingUnitFunction.Parse(message.Message.Function).Map(),
                    Function = message.Message.Function,
                    OsloGeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                    GeometryMethod = message.Message.GeometryMethod,
                    Geometry = sysGeometry,
                    HasDeviation = message.Message.HasDeviation,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.BuildingUnitNamespace,
                    Puri = $"{options.Value.BuildingUnitNamespace}/{message.Message.BuildingUnitPersistentLocalId}"
                };

                await context
                    .BuildingUnitLatestItems
                    .AddAsync(buildingUnitLatestItem, ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.OsloStatus = BuildingUnitStatus.Realized.Map();
                        buildingUnit.Status = BuildingUnitStatus.Realized.Status;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.OsloStatus = BuildingUnitStatus.Realized.Map();
                        buildingUnit.Status = BuildingUnitStatus.Realized.Status;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.OsloStatus = BuildingUnitStatus.Planned.Map();
                        buildingUnit.Status = BuildingUnitStatus.Planned.Status;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.OsloStatus = BuildingUnitStatus.Planned.Map();
                        buildingUnit.Status = BuildingUnitStatus.Planned.Status;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.OsloStatus = BuildingUnitStatus.Planned.Map();
                        buildingUnit.Status = BuildingUnitStatus.Planned.Status;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
                        buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.OsloStatus = BuildingUnitStatus.Realized.Map();
                        buildingUnit.Status = BuildingUnitStatus.Realized.Status;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
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
                    buildingUnit =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnit.OsloStatus = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Map();
                        buildingUnit.Status = message.Message.BuildingUnitStatus;
                        buildingUnit.OsloFunction = BuildingUnitFunction.Parse(message.Message.Function).Map();
                        buildingUnit.Function = message.Message.Function;
                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                        buildingUnit.GeometryMethod = message.Message.GeometryMethod;
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
                    Status = message.Message.BuildingUnitStatus,
                    OsloStatus = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Map(),
                    OsloFunction = BuildingUnitFunction.Common.Map(),
                    Function = BuildingUnitFunction.Common.Function,
                    OsloGeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                    GeometryMethod = message.Message.GeometryMethod,
                    Geometry = sysGeometry,
                    HasDeviation = message.Message.HasDeviation,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.BuildingUnitNamespace,
                    Puri = $"{options.Value.BuildingUnitNamespace}/{message.Message.BuildingUnitPersistentLocalId}"
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
                    buildingUnit =>
                    {
                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                        buildingUnit.GeometryMethod = message.Message.GeometryMethod;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
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
                    async buildingUnit =>
                    {
                        var previousAddress = await context.BuildingUnitAddresses.FindAsync(
                            [buildingUnit.BuildingUnitPersistentLocalId, message.Message.PreviousAddressPersistentLocalId], ct);

                        if (previousAddress is not null && previousAddress.Count == 1)
                        {
                            context.BuildingUnitAddresses.Remove(previousAddress);
                        }
                        else if (previousAddress is not null)
                        {
                            previousAddress.Count -= 1;
                        }

                        var newAddress = await context.BuildingUnitAddresses.FindAsync(
                            [buildingUnit.BuildingUnitPersistentLocalId, message.Message.NewAddressPersistentLocalId], ct);

                        if (newAddress is null || context.Entry(newAddress).State == EntityState.Deleted)
                        {
                            await context
                                .BuildingUnitAddresses
                                .AddAsync(new BuildingUnitAddress
                                {
                                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                                    AddressPersistentLocalId = message.Message.NewAddressPersistentLocalId
                                }, ct);
                        }
                        else
                        {
                            newAddress.Count += 1;
                        }

                        UpdateVersionTimestamp(buildingUnit, message.Message);
                    },
                    ct);
            });

            When<Envelope<BuildingBuildingUnitsAddressesWereReaddressed>>(async (context, message, ct) =>
            {
                foreach (var buildingUnitReaddresses in message.Message.BuildingUnitsReaddresses)
                {
                    await context.FindAndUpdateBuildingUnit(
                        buildingUnitReaddresses.BuildingUnitPersistentLocalId,
                        async buildingUnit =>
                        {
                            foreach (var addressPersistentLocalId in buildingUnitReaddresses.DetachedAddressPersistentLocalIds)
                            {
                                await context.RemoveIdempotentBuildingUnitAddress(buildingUnit, addressPersistentLocalId, ct);
                            }

                            foreach (var addressPersistentLocalId in buildingUnitReaddresses.AttachedAddressPersistentLocalIds)
                            {
                                await context.AddIdempotentBuildingUnitAddress(buildingUnit, addressPersistentLocalId, ct);
                            }

                            UpdateVersionTimestamp(buildingUnit, message.Message);
                        },
                        ct);
                }
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    async buildingUnit =>
                    {
                        var previousAddress = await context.BuildingUnitAddresses.FindAsync(
                            [buildingUnit.BuildingUnitPersistentLocalId, message.Message.PreviousAddressPersistentLocalId], ct);

                        if (previousAddress is not null)
                        {
                            context.BuildingUnitAddresses.Remove(previousAddress);
                        }

                        var newAddress = await context.BuildingUnitAddresses.FindAsync(
                            [buildingUnit.BuildingUnitPersistentLocalId, message.Message.NewAddressPersistentLocalId], ct);

                        if (newAddress is null || context.Entry(newAddress).State == EntityState.Deleted)
                        {
                            await context
                                .BuildingUnitAddresses
                                .AddAsync(new BuildingUnitAddress
                                {
                                    BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                                    AddressPersistentLocalId = message.Message.NewAddressPersistentLocalId
                                }, ct);
                        }

                        UpdateVersionTimestamp(buildingUnit, message.Message);
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
                        buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                        UpdateVersionTimestamp(buildingUnit, message.Message);
                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasMovedIntoBuilding>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnit(
                    message.Message.BuildingUnitPersistentLocalId,
                    buildingUnit =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnit.BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId;
                        buildingUnit.OsloStatus = BuildingUnitStatus.Parse(message.Message.BuildingUnitStatus).Map();
                        buildingUnit.Status = message.Message.BuildingUnitStatus;
                        buildingUnit.OsloFunction = BuildingUnitFunction.Parse(message.Message.Function).Map();
                        buildingUnit.Function = message.Message.Function;
                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                        buildingUnit.GeometryMethod = message.Message.GeometryMethod;
                        buildingUnit.HasDeviation = message.Message.HasDeviation;
                        UpdateVersionTimestamp(buildingUnit, message.Message);

                        return Task.CompletedTask;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasMovedOutOfBuilding>>(DoNothing);
        }

        private static void UpdateVersionTimestamp(BuildingUnitLatestItem building, IHasProvenance message)
            => building.VersionTimestamp = message.Provenance.Timestamp;

        private static Task DoNothing<T>(IntegrationContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
