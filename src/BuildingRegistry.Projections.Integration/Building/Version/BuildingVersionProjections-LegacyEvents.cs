namespace BuildingRegistry.Projections.Integration.Building.Version
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using BuildingRegistry.Building;
    using Converters;
    using Infrastructure;
    using Legacy.Events;
    using NetTopologySuite.IO;

    public sealed partial class BuildingVersionProjections
    {
        private void RegisterLegacyEvents(
            IntegrationOptions options,
            IPersistentLocalIdFinder persistentLocalIdFinder,
            WKBReader wkbReader)
        {
            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
            {
                var buildingPersistentLocalId = await persistentLocalIdFinder.FindBuildingPersistentLocalId(
                    message.Message.BuildingId, ct);

                if (buildingPersistentLocalId is null)
                {
                    throw new InvalidOperationException($"No persistent local id found for {message.Message.BuildingId}");
                }

                var building = new BuildingVersion
                {
                    Position = message.Position,
                    BuildingPersistentLocalId = buildingPersistentLocalId.Value,
                    BuildingId = message.Message.BuildingId,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    CreatedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.BuildingNamespace,
                    PuriId = $"{options.BuildingNamespace}/{buildingPersistentLocalId}"
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
                        building.Status = BuildingStatus.UnderConstruction.Value;
                        building.OsloStatus = BuildingStatus.UnderConstruction.Map();
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
                        building.Status = BuildingStatus.NotRealized.Value;
                        building.OsloStatus = BuildingStatus.NotRealized.Map();
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
                        building.Status = BuildingStatus.Planned.Value;
                        building.OsloStatus = BuildingStatus.Planned.Map();
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
                        building.Status = BuildingStatus.Realized.Value;
                        building.OsloStatus = BuildingStatus.Realized.Map();
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
                        building.Status = BuildingStatus.Retired.Value;
                        building.OsloStatus = BuildingStatus.Retired.Map();
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
                        building.Status = BuildingStatus.NotRealized.Value;
                        building.OsloStatus = BuildingStatus.NotRealized.Map();
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
                        building.Status = BuildingStatus.Planned.Value;
                        building.OsloStatus = BuildingStatus.Planned.Map();
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
                        building.Status = BuildingStatus.Retired.Value;
                        building.OsloStatus = BuildingStatus.Retired.Map();
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
                        building.Status = BuildingStatus.Realized.Value;
                        building.OsloStatus = BuildingStatus.Realized.Map();
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
                        building.Status = BuildingStatus.UnderConstruction.Value;
                        building.OsloStatus = BuildingStatus.UnderConstruction.Map();
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
                        building.OsloStatus = null;
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
                        building.OsloStatus = null;
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
                        building.OsloGeometryMethod = null;
                        building.NisCode = null;
                    },
                    ct);
            });

            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindMostIntersectingNisCodeBy(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Value;
                        building.OsloGeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map();
                        building.NisCode = nisCode;
                    },
                    ct);
            });

            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindMostIntersectingNisCodeBy(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.Outlined.Value;
                        building.OsloGeometryMethod = BuildingGeometryMethod.Outlined.Map();
                        building.NisCode = nisCode;
                    },
                    ct);
            });

            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindMostIntersectingNisCodeBy(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Value;
                        building.OsloGeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map();
                        building.NisCode = nisCode;
                    },
                    ct);
            });

            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                var nisCode = await context.FindMostIntersectingNisCodeBy(sysGeometry, ct);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.Outlined.Value;
                        building.OsloGeometryMethod = BuildingGeometryMethod.Outlined.Map();
                        building.NisCode = nisCode;
                    },
                    ct);
            });

            #endregion
        }
    }
}
