namespace BuildingRegistry.Projections.Integration.Building.Version
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using BuildingRegistry.Building.Events;
    using Converters;
    using Infrastructure;
    using Legacy.Events;
    using Microsoft.Extensions.Options;

    [ConnectedProjectionName("Integratie gebouw versie")]
    [ConnectedProjectionDescription("Projectie die de laatste gebouw data voor de integratie database bijhoudt.")]
    public sealed class BuildingVersionProjections : ConnectedProjection<IntegrationContext>
    {
        public BuildingVersionProjections(
            IOptions<IntegrationOptions> options,
            IPersistentLocalIdFinder persistentLocalIdFinder)
        {
            var wkbReader = WKBReaderFactory.Create();

            #region Legacy

            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
            {
                var buildingPersistentLocalId = await persistentLocalIdFinder.FindBuildingPersistentLocalId(
                    message.Message.BuildingId, ct);

                if (buildingPersistentLocalId is null)
                {
                    return;
                }

                var building = new BuildingVersion
                {
                    Position = message.Position,
                    BuildingPersistentLocalId = buildingPersistentLocalId.Value,
                    BuildingId = message.Message.BuildingId,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.BuildingNamespace,
                    PuriId = $"{options.Value.BuildingNamespace}/{buildingPersistentLocalId}"
                };

                await context
                    .BuildingVersions
                    .AddAsync(building, ct);
            });

            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.BuildingPersistentLocalId = message.Message.PersistentLocalId;
                    },
                    ct);
            });

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.IsRemoved = true;
                    },
                    ct);
            });

            #region Complete/Incomplete

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    _ => { },
                    ct);
            });

            #endregion

            #region Status

            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.UnderConstruction.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.NotRealized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.Planned.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingWasRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.Realized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.NotRealized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.Planned.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.Realized.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Status = BuildingStatus.UnderConstruction.Map();
                    },
                    ct);
            });

            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Status = null;
                    },
                    ct);
            });

            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Status = null;
                    },
                    ct);
            });

            #endregion

            #region Geometry

            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = null;
                        building.GeometryMethod = null;
                        building.NisCode = null;
                    },
                    ct);
            });

            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map();
                        building.NisCode = nisCode;
                    },
                    ct);
            });

            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.Outlined.Map();
                        building.NisCode = nisCode;
                    },
                    ct);
            });

            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map();
                        building.NisCode = nisCode;
                    },
                    ct);
            });

            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.Outlined.Map();
                        building.NisCode = nisCode;
                    },
                    ct);
            });

            #endregion

            #endregion

            When<Envelope<BuildingWasMigrated>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindNiscode(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.BuildingPersistentLocalId = message.Message.BuildingPersistentLocalId;
                        building.Status = BuildingStatus.Parse(message.Message.BuildingStatus).Map();
                        building.GeometryMethod = BuildingGeometryMethod.Parse(message.Message.GeometryMethod).Map();
                        building.Geometry = sysGeometry;
                        building.NisCode = nisCode;
                        building.IsRemoved = message.Message.IsRemoved;
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                        building.Namespace = options.Value.BuildingNamespace;
                        building.PuriId = $"{options.Value.BuildingNamespace}/{message.Message.BuildingPersistentLocalId}";
                    },
                    ct);
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
                    building => { building.Status = BuildingStatus.UnderConstruction.Map(); },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedFromUnderConstructionToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building => { building.Status = BuildingStatus.Planned.Map(); },
                    ct);
            });

            When<Envelope<BuildingWasRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building => { building.Status = BuildingStatus.Realized.Map(); },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedFromRealizedToUnderConstruction>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building => { building.Status = BuildingStatus.UnderConstruction.Map(); },
                    ct);
            });

            When<Envelope<BuildingWasNotRealizedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building => { building.Status = BuildingStatus.NotRealized.Map(); },
                    ct);
            });

            When<Envelope<BuildingWasCorrectedFromNotRealizedToPlanned>>(async (context, message, ct) =>
            {
                await context.CreateNewBuildingVersion(
                    message.Message.BuildingPersistentLocalId,
                    message,
                    building => { building.Status = BuildingStatus.Planned.Map(); },
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
                    building => { building.Status = BuildingStatus.Retired.Map(); },
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
                    building => { building.Status = BuildingStatus.Retired.Map(); },
                    ct);
            });
        }
    }
}
