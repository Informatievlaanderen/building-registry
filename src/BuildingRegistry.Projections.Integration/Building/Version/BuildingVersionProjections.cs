namespace BuildingRegistry.Projections.Integration.Building.Version
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Converters;
    using Infrastructure;
    using Microsoft.Extensions.Options;
    
    [ConnectedProjectionName("Integratie gebouw versie")]
    [ConnectedProjectionDescription("Projectie die de versie gebouw data voor de integratie database bijhoudt.")]
    public sealed partial class BuildingVersionProjections : ConnectedProjection<IntegrationContext>
    {
        public BuildingVersionProjections(
            IOptions<IntegrationOptions> options,
            IPersistentLocalIdFinder persistentLocalIdFinder,
            BuildingRegistry.Projections.Integration.IAddresses addresses)
        {
            var wkbReader = WKBReaderFactory.Create();

            RegisterBuildingUnitEvents(options.Value);
            RegisterLegacyBuildingUnitEvents(options.Value, persistentLocalIdFinder, addresses, wkbReader);
            RegisterLegacyEvents(options.Value, persistentLocalIdFinder, wkbReader);

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var building = new BuildingVersion
                {
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Position = message.Position,
                    Status = BuildingStatus.Parse(message.Message.BuildingStatus).Value,
                    OsloStatus = BuildingStatus.Parse(message.Message.BuildingStatus).Map(),
                    GeometryMethod = BuildingGeometryMethod.Parse(message.Message.GeometryMethod).Value,
                    OsloGeometryMethod = BuildingGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                    Geometry = sysGeometry,
                    IsRemoved = message.Message.IsRemoved,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    LastChangedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.BuildingNamespace,
                    PuriId = $"{options.Value.BuildingNamespace}/{message.Message.BuildingPersistentLocalId}",
                    BuildingUnits = new Collection<BuildingUnitVersion>(),
                    Type = message.EventName
                };

                foreach (var buildingUnit in message.Message.BuildingUnits)
                {
                    var sysBuildingUnitGeometry = wkbReader.Read(buildingUnit.ExtendedWkbGeometry.ToByteArray());

                    var buildingUnitAddressVersions = buildingUnit.AddressPersistentLocalIds
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
                        Status = BuildingUnitStatus.Parse(buildingUnit.Status).Status,
                        OsloStatus = BuildingUnitStatus.Parse(buildingUnit.Status).Map(),
                        Function = BuildingUnitFunction.Parse(buildingUnit.Function).Function,
                        OsloFunction = BuildingUnitFunction.Parse(buildingUnit.Function).Map(),
                        GeometryMethod = BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod).GeometryMethod,
                        OsloGeometryMethod = BuildingUnitPositionGeometryMethod.Parse(buildingUnit.GeometryMethod).Map(),
                        Geometry = sysBuildingUnitGeometry,
                        HasDeviation = false,
                        IsRemoved = buildingUnit.IsRemoved,
                        VersionTimestamp = message.Message.Provenance.Timestamp,
                        CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                        Namespace = options.Value.BuildingUnitNamespace,
                        PuriId = $"{options.Value.BuildingUnitNamespace}/{buildingUnit.BuildingUnitPersistentLocalId}",
                        Addresses = new Collection<BuildingUnitAddressVersion>(buildingUnitAddressVersions),
                        Type = message.EventName
                    };

                    building.BuildingUnits.Add(buildingUnitVersion);
                }

                await context
                    .BuildingVersions
                    .AddAsync(building, ct);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var building = new BuildingVersion
                {
                    Position = message.Position,
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Status = BuildingStatus.Planned.Value,
                    OsloStatus = BuildingStatus.Planned.Map(),
                    GeometryMethod = BuildingGeometryMethod.Outlined.Value,
                    OsloGeometryMethod = BuildingGeometryMethod.Outlined.Map(),
                    Geometry = sysGeometry,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    LastChangedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.BuildingNamespace,
                    PuriId = $"{options.Value.BuildingNamespace}/{message.Message.BuildingPersistentLocalId}",
                    Type = message.EventName

                };

                await context
                    .BuildingVersions
                    .AddAsync(building, ct);
            });

            When<Envelope<UnplannedBuildingWasRealizedAndMeasured>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var building = new BuildingVersion
                {
                    Position = message.Position,
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Status = BuildingStatus.Realized.Value,
                    OsloStatus = BuildingStatus.Realized.Map(),
                    GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Value,
                    OsloGeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map(),
                    Geometry = sysGeometry,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    LastChangedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.BuildingNamespace,
                    PuriId = $"{options.Value.BuildingNamespace}/{message.Message.BuildingPersistentLocalId}",
                    Type = message.EventName
                };

                await context
                    .BuildingVersions
                    .AddAsync(building, ct);
            });

            When<Envelope<BuildingOutlineWasChanged>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.Outlined.Value;
                        building.OsloGeometryMethod = BuildingGeometryMethod.Outlined.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;

                        if (!message.Message.BuildingUnitPersistentLocalIds.Any())
                        {
                            return;
                        }

                        var sysBuildingUnitGeometry = wkbReader.Read(message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray());

                        foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds)
                        {
                            var buildingUnit = building.BuildingUnits
                                .Single(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

                            buildingUnit.Geometry = sysBuildingUnitGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod;
                            buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Value;
                        building.OsloGeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;

                        if (!message.Message.BuildingUnitPersistentLocalIds.Any()
                            && !message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived.Any())
                        {
                            return;
                        }

                        var sysBuildingUnitGeometry = wkbReader.Read(message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray());

                        foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds
                                     .Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                        {
                            var buildingUnit = building.BuildingUnits
                                .Single(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

                            buildingUnit.Geometry = sysBuildingUnitGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod;
                            buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingBecameUnderConstructionV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.UnderConstruction.Value;
                        building.OsloStatus = BuildingStatus.UnderConstruction.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.Planned.Value;
                        building.OsloStatus = BuildingStatus.Planned.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.Realized.Value;
                        building.OsloStatus = BuildingStatus.Realized.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.UnderConstruction.Value;
                        building.OsloStatus = BuildingStatus.UnderConstruction.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.NotRealized.Value;
                        building.OsloStatus = BuildingStatus.NotRealized.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.Planned.Value;
                        building.OsloStatus = BuildingStatus.Planned.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Value;
                        building.OsloGeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;

                        if (!message.Message.BuildingUnitPersistentLocalIds.Any()
                            && !message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived.Any())
                        {
                            return;
                        }

                        var sysBuildingUnitGeometry = wkbReader.Read(message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray());

                        foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds
                                     .Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                        {
                            var buildingUnit = building.BuildingUnits
                                .Single(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

                            buildingUnit.Geometry = sysBuildingUnitGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod;
                            buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Value;
                        building.OsloGeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;

                        if (!message.Message.BuildingUnitPersistentLocalIds.Any()
                            && !message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived.Any())
                        {
                            return;
                        }

                        var sysBuildingUnitGeometry = wkbReader.Read(message.Message.ExtendedWkbGeometryBuildingUnits!.ToByteArray());

                        foreach (var buildingUnitPersistentLocalId in message.Message.BuildingUnitPersistentLocalIds
                                     .Concat(message.Message.BuildingUnitPersistentLocalIdsWhichBecameDerived))
                        {
                            var buildingUnit = building.BuildingUnits
                                .Single(x => x.BuildingUnitPersistentLocalId == buildingUnitPersistentLocalId);

                            buildingUnit.Geometry = sysBuildingUnitGeometry;
                            buildingUnit.GeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.GeometryMethod;
                            buildingUnit.OsloGeometryMethod = BuildingUnitPositionGeometryMethod.DerivedFromObject.Map();
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }
                    },
                    ct);
            });

            When<Envelope<BuildingWasDemolished>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.Retired.Value;
                        building.OsloStatus = BuildingStatus.Retired.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.IsRemoved = true;
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            // When<Envelope<BuildingMergerWasRealized>>(async (context, message, ct) =>
            // {
            //     var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
            //     var sysGeometry = wkbReader.Read(geometryAsBinary);
            //
            //     var building = new BuildingVersion
            //     {
            //         Position = message.Position,
            //         BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
            //         Status = BuildingStatus.Realized.Value,
            //         OsloStatus = BuildingStatus.Realized.Map(),
            //         GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Value,
            //         OsloGeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map(),
            //         Geometry = sysGeometry,
            //         IsRemoved = false,
            //         VersionTimestamp = message.Message.Provenance.Timestamp,
            //         CreatedOnTimestamp = message.Message.Provenance.Timestamp,
            //         LastChangedOnTimestamp = message.Message.Provenance.Timestamp,
            //         Namespace = options.Value.BuildingNamespace,
            //         PuriId = $"{options.Value.BuildingNamespace}/{message.Message.BuildingPersistentLocalId}"
            //     };
            //
            //     await context
            //         .BuildingVersions
            //         .AddAsync(building, ct);
            // });
            //
            // When<Envelope<BuildingWasMerged>>(async (context, message, ct) =>
            // {
            //     await context.CreateNewBuildingVersion(
            //         message.Message.BuildingPersistentLocalId,
            //         message,
            //         building =>
            //         {
            //             building.Status = BuildingStatus.Retired.Value;
            //             building.OsloStatus = BuildingStatus.Retired.Map();
            //             building.VersionTimestamp = message.Message.Provenance.Timestamp;
            //         },
            //         ct);
            // });
        }
    }
}
