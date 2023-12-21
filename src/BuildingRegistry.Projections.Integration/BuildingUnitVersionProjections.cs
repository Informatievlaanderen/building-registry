namespace BuildingRegistry.Projections.Integration
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
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
    public sealed class BuildingUnitVersionProjections : ConnectedProjection<IntegrationContext>
    {
        public BuildingUnitVersionProjections(IOptions<IntegrationOptions> options)
        {
            var wkbReader = WKBReaderFactory.Create();

            #region Legacy



            #endregion

            #region Building

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    var geometryAsBinary = buildingUnit.ExtendedWkbGeometry.ToByteArray();
                    var sysGeometry = wkbReader.Read(geometryAsBinary);

                    var addresses = buildingUnit.AddressPersistentLocalIds
                        .Distinct()
                        .Select(x => new BuildingUnitAddressVersion
                        {
                            Position = message.Position,
                            BuildingUnitPersistentLocalId = buildingUnit.BuildingUnitPersistentLocalId,
                            AddressPersistentLocalId = x,
                        })
                        .ToList();

                    var buildingUnitVersion = new BuildingUnitVersion
                    {
                        Position = message.Position,
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
                        Addresses = new Collection<BuildingUnitAddressVersion>(addresses)
                    };

                    await context
                        .BuildingUnitVersions
                        .AddAsync(buildingUnitVersion, ct);
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
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitPersistentLocalId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
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
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitPersistentLocalId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
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
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitPersistentLocalId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
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
                    await context.CreateNewBuildingUnitVersion(
                        buildingUnitPersistentLocalId,
                        message,
                        buildingUnit =>
                        {
                            buildingUnit.Geometry = sysGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                        },
                        ct);
                }
            });

            #endregion

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var buildingUnitVersion = new BuildingUnitVersion
                {
                    Position = message.Position,
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
                };

                await context
                    .BuildingUnitVersions
                    .AddAsync(buildingUnitVersion, ct);
            });

            When<Envelope<BuildingUnitWasRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRealizedBecauseBuildingWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRealizedToPlannedBecauseBuildingWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.NotRealized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Planned.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasCorrectedFromRetiredToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingUnitStatus.Realized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.IsRemoved = true;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemovedBecauseBuildingWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.IsRemoved = true;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
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
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRegularized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = false;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = true;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasDeregulated>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = true;
                    },
                    ct);
            });

            When<Envelope<BuildingUnitDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.HasDeviation = false;
                    },
                    ct);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var buildingUnitVersion = new BuildingUnitVersion
                {
                    Position = message.Position,
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
                    PuriId = $"{options.Value.BuildingUnitNamespace}/{message.Message.BuildingUnitPersistentLocalId}"
                };

                await context
                    .BuildingUnitVersions
                    .AddAsync(buildingUnitVersion, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasAttachedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Addresses.Add(new BuildingUnitAddressVersion
                        {
                            AddressPersistentLocalId = message.Message.AddressPersistentLocalId,
                            BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                            Position = message.Position
                        });
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasDetachedBecauseAddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.AddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingUnitAddressWasReplacedBecauseAddressWasReaddressed>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var address = buildingUnit.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == message.Message.PreviousAddressPersistentLocalId);
                        if (address is not null)
                        {
                            buildingUnit.Addresses.Remove(address);
                        }

                        buildingUnit.Addresses.Add(new BuildingUnitAddressVersion
                        {
                            AddressPersistentLocalId = message.Message.NewAddressPersistentLocalId,
                            BuildingUnitPersistentLocalId = message.Message.BuildingUnitPersistentLocalId,
                            Position = message.Position
                        });

                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasRetiredBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedBecauseBuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        buildingUnit.Status = BuildingStatus.NotRealized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingUnitWasTransferred>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingUnitVersion(
                    message.Message.BuildingUnitPersistentLocalId,
                    message,
                    buildingUnit =>
                    {
                        var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                        var sysGeometry = wkbReader.Read(geometryAsBinary);

                        buildingUnit.BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId;
                        buildingUnit.Status = BuildingUnitStatus.Parse(message.Message.Status).Map();
                        buildingUnit.Function = BuildingUnitFunction.Parse(message.Message.Function).Map();
                        buildingUnit.Geometry = sysGeometry;
                        buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                        buildingUnit.HasDeviation = message.Message.HasDeviation;
                    },
                    ct);
            });

            // BuildingUnitWasTransferred couples the unit to another building and BuildingUnitMoved is an event applicable on the old building.
            When<Envelope<BuildingUnitWasMoved>>((_, _, _) => Task.CompletedTask);
        }
    }
}
