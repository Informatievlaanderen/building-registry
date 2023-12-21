namespace BuildingRegistry.Projections.Integration
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building;
    using Building.Events;
    using Converters;
    using Infrastructure;
    using Microsoft.Extensions.Options;

    [ConnectedProjectionName("Integratie gebouw versie")]
    [ConnectedProjectionDescription("Projectie die de laatste gebouw data voor de integratie database bijhoudt.")]
    public sealed class BuildingVersionProjections : ConnectedProjection<IntegrationContext>
    {
        public BuildingVersionProjections(IOptions<IntegrationOptions> options)
        {
            var wkbReader = WKBReaderFactory.Create();

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                var building = new BuildingVersion
                {
                    Position = message.Position,
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Status = BuildingStatus.Parse(message.Message.BuildingStatus).Map(),
                    GeometryMethod = BuildingGeometryMethod.Parse(message.Message.GeometryMethod).Map(),
                    Geometry = sysGeometry,
                    NisCode = nisCode,
                    IsRemoved = message.Message.IsRemoved,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.BuildingNamespace,
                    PuriId = $"{options.Value.BuildingNamespace}/{message.Message.BuildingPersistentLocalId}"
                };

                await context
                    .BuildingVersions
                    .AddAsync(building, ct);
            });

            When<Envelope<BuildingWasPlannedV2>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                var building = new BuildingVersion
                {
                    Position = message.Position,
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Status = BuildingStatus.Planned.Map(),
                    GeometryMethod = BuildingGeometryMethod.Outlined.Map(),
                    Geometry = sysGeometry,
                    NisCode = nisCode,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
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

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                var building = new BuildingVersion
                {
                    Position = message.Position,
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Status = BuildingStatus.Realized.Map(),
                    GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map(),
                    Geometry = sysGeometry,
                    NisCode = nisCode,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
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

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.Outlined.Map();
                        building.NisCode = nisCode;
                    },
                    ct);
            });

            When<Envelope<BuildingMeasurementWasChanged>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map();
                        building.NisCode = nisCode;
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
                        building.Status = BuildingStatus.UnderConstruction.Map();
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
                        building.Status = BuildingStatus.Planned.Map();
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
                        building.Status = BuildingStatus.Realized.Map();
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
                        building.Status = BuildingStatus.UnderConstruction.Map();
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
                        building.Status = BuildingStatus.NotRealized.Map();
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
                        building.Status = BuildingStatus.Planned.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingWasMeasured>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map();
                        building.NisCode = nisCode;
                    },
                    ct);
            });

            When<Envelope<BuildingMeasurementWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometryBuilding.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map();
                        building.NisCode = nisCode;
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
                        building.Status = BuildingStatus.Retired.Map();
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
                    },
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
                    _ => {  },
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

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                var building = new BuildingVersion
                {
                    Position = message.Position,
                    BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId,
                    Status = BuildingStatus.Realized.Map(),
                    GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map(),
                    Geometry = sysGeometry,
                    NisCode = nisCode,
                    IsRemoved = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
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
                        building.Status = BuildingStatus.Retired.Map();
                    },
                    ct);
            });
        }
    }
}
