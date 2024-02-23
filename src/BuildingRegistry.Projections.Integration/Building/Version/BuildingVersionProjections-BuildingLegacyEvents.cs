#pragma warning disable CS0618 // Type or member is obsolete
namespace BuildingRegistry.Projections.Integration.Building.Version
{
    using System;
    using System.Linq;
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
                    message.Message.BuildingId);

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
                    LastChangedOnTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.BuildingNamespace,
                    PuriId = $"{options.BuildingNamespace}/{buildingPersistentLocalId}",
                    Type = message.EventName
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
                        foreach (var buildingUnitVersion in building.BuildingUnits)
                        {
                            buildingUnitVersion.BuildingPersistentLocalId = message.Message.PersistentLocalId;
                            buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }

                        building.BuildingPersistentLocalId = message.Message.PersistentLocalId;
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        foreach (var buildingUnitId in message.Message.BuildingUnitIds)
                        {
                            var buildingUnitVersion = building.BuildingUnits.Single(x => x.BuildingUnitId == buildingUnitId);

                            buildingUnitVersion.IsRemoved = true;
                            buildingUnitVersion.Addresses.Clear();
                            buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }

                        building.IsRemoved = true;
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

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
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        foreach (var buildingUnitId in message.Message.BuildingUnitIdsToNotRealize)
                        {
                            var buildingUnit = building.BuildingUnits.Single(x => x.BuildingUnitId == buildingUnitId);

                            buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                            buildingUnit.Addresses.Clear();
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }

                        foreach (var buildingUnitId in message.Message.BuildingUnitIdsToRetire)
                        {
                            var buildingUnit = building.BuildingUnits.Single(x => x.BuildingUnitId == buildingUnitId);

                            buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
                            buildingUnit.Addresses.Clear();
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }

                        building.Status = BuildingStatus.NotRealized.Value;
                        building.OsloStatus = BuildingStatus.NotRealized.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        foreach (var buildingUnitId in message.Message.BuildingUnitIdsToNotRealize)
                        {
                            var buildingUnit = building.BuildingUnits.Single(x => x.BuildingUnitId == buildingUnitId);

                            buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                            buildingUnit.Addresses.Clear();
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }

                        foreach (var buildingUnitId in message.Message.BuildingUnitIdsToRetire)
                        {
                            var buildingUnit = building.BuildingUnits.Single(x => x.BuildingUnitId == buildingUnitId);

                            buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
                            buildingUnit.Addresses.Clear();
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }

                        building.Status = BuildingStatus.Retired.Value;
                        building.OsloStatus = BuildingStatus.Retired.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        foreach (var buildingUnitId in message.Message.BuildingUnitIdsToNotRealize)
                        {
                            var buildingUnit = building.BuildingUnits.Single(x => x.BuildingUnitId == buildingUnitId);

                            buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                            buildingUnit.Addresses.Clear();
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }

                        foreach (var buildingUnitId in message.Message.BuildingUnitIdsToRetire)
                        {
                            var buildingUnit = building.BuildingUnits.Single(x => x.BuildingUnitId == buildingUnitId);

                            buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
                            buildingUnit.Addresses.Clear();
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }

                        building.Status = BuildingStatus.NotRealized.Value;
                        building.OsloStatus = BuildingStatus.NotRealized.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        foreach (var buildingUnitId in message.Message.BuildingUnitIdsToNotRealize)
                        {
                            var buildingUnit = building.BuildingUnits.Single(x => x.BuildingUnitId == buildingUnitId);

                            buildingUnit.Status = BuildingUnitStatus.NotRealized.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.NotRealized.Map();
                            buildingUnit.Addresses.Clear();
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }

                        foreach (var buildingUnitId in message.Message.BuildingUnitIdsToRetire)
                        {
                            var buildingUnit = building.BuildingUnits.Single(x => x.BuildingUnitId == buildingUnitId);

                            buildingUnit.Status = BuildingUnitStatus.Retired.Status;
                            buildingUnit.OsloStatus = BuildingUnitStatus.Retired.Map();
                            buildingUnit.Addresses.Clear();
                            buildingUnit.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }

                        building.Status = BuildingStatus.Retired.Value;
                        building.OsloStatus = BuildingStatus.Retired.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
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
                        foreach (var buildingUnitVersion in building.BuildingUnits)
                        {
                            buildingUnitVersion.Geometry = null;
                            buildingUnitVersion.GeometryMethod = null;
                            buildingUnitVersion.OsloGeometryMethod = null;
                            buildingUnitVersion.VersionTimestamp = message.Message.Provenance.Timestamp;
                        }

                        building.Geometry = null;
                        building.GeometryMethod = null;
                        building.OsloGeometryMethod = null;
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Value;
                        building.OsloGeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.Outlined.Value;
                        building.OsloGeometryMethod = BuildingGeometryMethod.Outlined.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Value;
                        building.OsloGeometryMethod = BuildingGeometryMethod.MeasuredByGrb.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) =>
            {
                var geometryAsBinary = message.Message.ExtendedWkbGeometry.ToByteArray();
                var sysGeometry = wkbReader.Read(geometryAsBinary);

                await context.CreateNewBuildingVersion(
                    message.Message.BuildingId,
                    message,
                    building =>
                    {
                        building.Geometry = sysGeometry;
                        building.GeometryMethod = BuildingGeometryMethod.Outlined.Value;
                        building.OsloGeometryMethod = BuildingGeometryMethod.Outlined.Map();
                        building.VersionTimestamp = message.Message.Provenance.Timestamp;
                    },
                    ct);
            });

            #endregion
        }
    }
}
