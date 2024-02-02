namespace BuildingRegistry.Projections.Integration.Building.Version
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Converters;
    using Infrastructure;
    using Microsoft.Extensions.Options;

    [ConnectedProjectionName("Integratie gebouw versie")]
    [ConnectedProjectionDescription("Projectie die de laatste gebouw data voor de integratie database bijhoudt.")]
    public sealed partial class BuildingVersionProjections : ConnectedProjection<IntegrationContext>
    {
        public BuildingVersionProjections(
            IOptions<IntegrationOptions> options,
            IPersistentLocalIdFinder persistentLocalIdFinder)
        {
            var wkbReader = WKBReaderFactory.Create();

            RegisterLegacyEvents(options.Value, persistentLocalIdFinder, wkbReader);

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var building = new BuildingVersion
                {
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Position = message.Position,
                    BuildingId = message.Message.BuildingId,
                    Status = BuildingStatus.Parse(message.Message.BuildingStatus).Value,
                    OsloStatus = BuildingStatus.Parse(message.Message.BuildingStatus).Map(),
                    GeometryMethod = BuildingGeometryMethod.Parse(message.Message.GeometryMethod).Value,
                    OsloGeometryMethod = BuildingGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                    Geometry = sysGeometry,
                    IsRemoved = message.Message.IsRemoved,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.BuildingNamespace,
                    PuriId = $"{options.Value.BuildingNamespace}/{message.Message.BuildingPersistentLocalId}",
                };

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
                    Namespace = options.Value.BuildingNamespace,
                    PuriId = $"{options.Value.BuildingNamespace}/{message.Message.BuildingPersistentLocalId}"
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
                    Namespace = options.Value.BuildingNamespace,
                    PuriId = $"{options.Value.BuildingNamespace}/{message.Message.BuildingPersistentLocalId}"
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
                    },
                    ct);
            });

            When<Envelope<BuildingWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building => { building.IsRemoved = true; },
                    ct);
            });

            When<Envelope<BuildingUnitWasPlannedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<CommonBuildingUnitWasAddedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<BuildingUnitWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<BuildingUnitRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<BuildingMergerWasRealized>>(async (context, message, ct) =>
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
                    Namespace = options.Value.BuildingNamespace,
                    PuriId = $"{options.Value.BuildingNamespace}/{message.Message.BuildingPersistentLocalId}"
                };

                await context
                    .BuildingVersions
                    .AddAsync(building, ct);
            });

            When<Envelope<BuildingUnitWasTransferred>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<BuildingUnitWasMoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<BuildingWasMerged>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.Retired.Value;
                        building.OsloStatus = BuildingStatus.Retired.Map();
                    },
                    ct);
            });
        }
    }
}
