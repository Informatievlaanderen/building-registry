namespace BuildingRegistry.Projections.Extract.BuildingUnitExtract
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Building.Events;
    using NetTopologySuite.IO;
    using NodaTime;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Building.Events.Crab;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.Geometries;
    using ValueObjects;
    using Point = Be.Vlaanderen.Basisregisters.Shaperon.Point;

    public class BuildingUnitExtractProjections : ConnectedProjection<ExtractContext>
    {
        private const string NotRealized = "NietGerealiseerd";
        private const string Planned = "Gepland";
        private const string Realized = "Gerealiseerd";
        private const string Retired = "Gehistoreerd";

        private const string Unknown = "NietGekend";
        private const string Common = "GemeenschappelijkDeel";

        private const string DerivedFromObject = "AfgeleidVanObject";
        private const string AppointedByAdministrator = "AangeduidDoorBeheerder";

        private readonly ExtractConfig _extractConfig;
        private readonly Encoding _encoding;

        public BuildingUnitExtractProjections(IOptions<ExtractConfig> extractConfig, Encoding encoding, WKBReader wkbReader)
        {
            _extractConfig = extractConfig.Value;
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));

            When<Envelope<CommonBuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                var building =
                    await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                await context
                    .BuildingUnitExtract
                    .AddAsync(new BuildingUnitExtractItem
                    {
                        BuildingUnitId = message.Message.BuildingUnitId,
                        BuildingId = message.Message.BuildingId,
                        IsComplete = false,
                        IsBuildingComplete = building.IsComplete ?? false,
                        DbaseRecord = new BuildingUnitDbaseRecord
                        {
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() },
                            functie = { Value = Common },
                            gebouwid = { Value = building.BuildingPersistentLocalId.HasValue ? building.BuildingPersistentLocalId.ToString() : null }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<BuildingUnitWasAdded>>(async (context, message, ct) =>
            {
                var building =
                    await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                await context
                    .BuildingUnitExtract
                    .AddAsync(new BuildingUnitExtractItem
                    {
                        BuildingUnitId = message.Message.BuildingUnitId,
                        BuildingId = message.Message.BuildingId,
                        IsComplete = false,
                        IsBuildingComplete = building.IsComplete ?? false,
                        DbaseRecord = new BuildingUnitDbaseRecord
                        {
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() },
                            functie = { Value = Unknown },
                            gebouwid = { Value = building.BuildingPersistentLocalId.HasValue ? building.BuildingPersistentLocalId.ToString() : null }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<BuildingUnitWasAddedToRetiredBuilding>>(async (context, message, ct) =>
            {
                var building =
                    await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                await context
                    .BuildingUnitExtract
                    .AddAsync(new BuildingUnitExtractItem
                    {
                        BuildingUnitId = message.Message.BuildingUnitId,
                        BuildingId = message.Message.BuildingId,
                        IsComplete = false,
                        IsBuildingComplete = building.IsComplete ?? false,
                        DbaseRecord = new BuildingUnitDbaseRecord
                        {
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() },
                            functie = { Value = Unknown },
                            gebouwid = { Value = building.BuildingPersistentLocalId.HasValue ? building.BuildingPersistentLocalId.ToString() : null },
                            status = { Value = Retired }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<BuildingUnitWasReaddedByOtherUnitRemoval>>(async (context, message, ct) =>
            {
                var building =
                    await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                await context
                    .BuildingUnitExtract
                    .AddAsync(new BuildingUnitExtractItem
                    {
                        BuildingUnitId = message.Message.BuildingUnitId,
                        BuildingId = message.Message.BuildingId,
                        IsComplete = false,
                        IsBuildingComplete = building.IsComplete ?? false,
                        DbaseRecord = new BuildingUnitDbaseRecord
                        {
                            versieid = { Value = message.Message.Provenance.Timestamp.ToBelgianDateTimeOffset().FromDateTimeOffset() },
                            functie = { Value = Unknown },
                            gebouwid = { Value = building.BuildingPersistentLocalId.HasValue ? building.BuildingPersistentLocalId.ToString() : null }
                        }.ToBytes(_encoding)
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRemoved>>(async (context, message, ct) =>
            {
                var item = await context.BuildingUnitExtract.FindAsync(message.Message.BuildingUnitId, cancellationToken: ct);
                context.BuildingUnitExtract.Remove(item);
            });

            When<Envelope<BuildingUnitPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        item.PersistentLocalId = message.Message.PersistentLocalId;
                        UpdateId(item, message.Message.PersistentLocalId);
                    }, ct);
            });

            When<Envelope<BuildingUnitBecameComplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        item.IsComplete = true;
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        item.IsComplete = false;
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitPositionWasAppointedByAdministrator>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        var geometry = wkbReader.Read(message.Message.Position.ToByteArray());

                        UpdateGeometry(item, geometry);
                        UpdateGeometryMethod(item, AppointedByAdministrator);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToAppointedByAdministrator>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        var geometry = wkbReader.Read(message.Message.Position.ToByteArray());

                        UpdateGeometry(item, geometry);
                        UpdateGeometryMethod(item, AppointedByAdministrator);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitPositionWasCorrectedToDerivedFromObject>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        var geometry = wkbReader.Read(message.Message.Position.ToByteArray());

                        UpdateGeometry(item, geometry);
                        UpdateGeometryMethod(item, DerivedFromObject);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitPositionWasDerivedFromObject>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        var geometry = wkbReader.Read(message.Message.Position.ToByteArray());

                        UpdateGeometry(item, geometry);
                        UpdateGeometryMethod(item, DerivedFromObject);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, null);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, NotRealized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Planned);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Realized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Retired);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, NotRealized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasNotRealizedByParent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, NotRealized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasPlanned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Planned);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRealized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Realized);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Retired);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitWasRetiredByParent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.BuildingUnitId,
                    item =>
                    {
                        UpdateStatus(item, Retired);
                        UpdateVersie(item, message.Message.Provenance.Timestamp);
                    }, ct);
            });

            When<Envelope<BuildingUnitAddressWasAttached>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.To,
                    item => UpdateVersie(item, message.Message.Provenance.Timestamp), ct);
            });

            When<Envelope<BuildingUnitAddressWasDetached>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateBuildingUnitExtract(message.Message.From,
                    item => UpdateVersie(item, message.Message.Provenance.Timestamp), ct);
            });

            When<Envelope<BuildingUnitPersistentLocalIdWasDuplicated>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitPersistentLocalIdWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingUnitWasReaddressed>>(async (context, message, ct) => DoNothing());

            #region Building

            When<Envelope<BuildingPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var units = GetBuildingUnitsByBuilding(context, message.Message.BuildingId);
                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.BuildingPersistentLocalId = message.Message.PersistentLocalId;

                foreach (var unit in units)
                    UpdateRecord(unit, item => { item.gebouwid.Value = message.Message.PersistentLocalId.ToString(); });
            });

            When<Envelope<BuildingBecameComplete>>(async (context, message, ct) =>
            {
                var units = GetBuildingUnitsByBuilding(context, message.Message.BuildingId);
                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.IsComplete = true;

                foreach (var buildingUnitExtractItem in units)
                {
                    UpdateRecord(buildingUnitExtractItem, b =>
                    {
                        buildingUnitExtractItem.IsBuildingComplete = true;
                        UpdateVersie(buildingUnitExtractItem, message.Message.Provenance.Timestamp);
                    });
                }
            });

            When<Envelope<BuildingBecameIncomplete>>(async (context, message, ct) =>
            {
                var units = GetBuildingUnitsByBuilding(context, message.Message.BuildingId);
                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.IsComplete = false;

                foreach (var buildingUnitExtractItem in units)
                {
                    UpdateRecord(buildingUnitExtractItem, b =>
                    {
                        buildingUnitExtractItem.IsBuildingComplete = true;
                        UpdateVersie(buildingUnitExtractItem, message.Message.Provenance.Timestamp);
                    });
                }
            });

            When<Envelope<BuildingGeometryWasRemoved>>(async (context, message, ct) =>
            {
                var units = GetBuildingUnitsByBuilding(context, message.Message.BuildingId);
                foreach (var buildingUnitExtractItem in units)
                {
                    UpdateRecord(buildingUnitExtractItem, b =>
                    {
                        UpdateGeometry(buildingUnitExtractItem, null);
                        UpdateGeometryMethod(buildingUnitExtractItem, null);
                        UpdateVersie(buildingUnitExtractItem, message.Message.Provenance.Timestamp);
                    });
                }
            });

            When<Envelope<BuildingWasCorrectedToNotRealized>>(async (context, message, ct) =>
            {
                var units = GetBuildingUnitsByBuilding(context, message.Message.BuildingId);
                RetireUnitsByBuilding(units, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp, context);

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.NotRealized;
            });

            When<Envelope<BuildingWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var units = GetBuildingUnitsByBuilding(context, message.Message.BuildingId);
                RetireUnitsByBuilding(units, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp, context);

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.Retired;
            });

            When<Envelope<BuildingWasNotRealized>>(async (context, message, ct) =>
            {
                var units = GetBuildingUnitsByBuilding(context, message.Message.BuildingId);
                RetireUnitsByBuilding(units, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp, context);

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.NotRealized;
            });

            When<Envelope<BuildingWasRemoved>>(async (context, message, ct) =>
            {
                var units = GetBuildingUnitsByBuilding(context, message.Message.BuildingId);
                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);

                building.IsRemoved = true;

                foreach (var buildingUnitExtractItem in units)
                    context.Remove(buildingUnitExtractItem);
            });

            When<Envelope<BuildingWasRetired>>(async (context, message, ct) =>
            {
                var units = GetBuildingUnitsByBuilding(context, message.Message.BuildingId);
                RetireUnitsByBuilding(units, message.Message.BuildingUnitIdsToNotRealize, message.Message.BuildingUnitIdsToRetire, message.Message.Provenance.Timestamp, context);

                var building = await context.BuildingUnitBuildings.FindAsync(message.Message.BuildingId, cancellationToken: ct);
                building.BuildingRetiredStatus = BuildingStatus.Retired;
            });

            When<Envelope<BuildingBecameUnderConstruction>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingMeasurementByGrbWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingOutlineWasCorrected>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingStatusWasCorrectedToRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingStatusWasRemoved>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasCorrectedToUnderConstruction>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasMeasuredByGrb>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasOutlined>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasPlanned>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasRealized>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .BuildingUnitBuildings
                    .AddAsync(
                        new BuildingUnitBuildingItem
                        {
                            BuildingId = message.Message.BuildingId,
                            IsRemoved = false
                        }, ct);
            });
            #endregion Building

            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingGeometryWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<BuildingStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<HouseNumberWasReaddressedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<SubaddressWasReaddressedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<TerrainObjectWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private static IEnumerable<BuildingUnitExtractItem> GetBuildingUnitsByBuilding(ExtractContext context, Guid buildingId)
        {
            var units = context.BuildingUnitExtract.Local.Where(x => x.BuildingId == buildingId)
                .Union(context.BuildingUnitExtract.Where(x => x.BuildingId == buildingId).ToList());
            return units;
        }

        private static void UpdateGeometry(BuildingUnitExtractItem item, Geometry geometry)
        {
            if (geometry == null)
            {
                item.ShapeRecordContentLength = 0;
                item.ShapeRecordContent = null;
                item.MinimumY = 0;
                item.MinimumX = 0;
                item.MaximumY = 0;
                item.MaximumX = 0;
            }
            else
            {
                var pointShapeContent = new PointShapeContent(new Point(geometry.Coordinate.X, geometry.Coordinate.Y));
                item.ShapeRecordContent = pointShapeContent.ToBytes();
                item.ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32();
                item.MinimumX = pointShapeContent.Shape.X;
                item.MaximumX = pointShapeContent.Shape.X;
                item.MinimumY = pointShapeContent.Shape.Y;
                item.MaximumY = pointShapeContent.Shape.Y;
            }
        }

        private void UpdateStatus(BuildingUnitExtractItem buildingUnit, string status)
            => UpdateRecord(buildingUnit, record => record.status.Value = status);

        private void UpdateGeometryMethod(BuildingUnitExtractItem buildingUnit, string method)
            => UpdateRecord(buildingUnit, record => record.posgeommet.Value = method);

        private void UpdateId(BuildingUnitExtractItem buildingUnit, int id)
            => UpdateRecord(buildingUnit, record =>
            {
                record.id.Value = $"{_extractConfig.DataVlaanderenNamespaceBuildingUnit}/{id}";
                record.gebouwehid.Value = id;
            });

        private void UpdateVersie(BuildingUnitExtractItem buildingUnit, Instant timestamp)
            => UpdateRecord(buildingUnit, record => record.versieid.SetValue(timestamp.ToBelgianDateTimeOffset()));

        private void UpdateRecord(BuildingUnitExtractItem buildingUnit, Action<BuildingUnitDbaseRecord> updateFunc)
        {
            var record = new BuildingUnitDbaseRecord();
            record.FromBytes(buildingUnit.DbaseRecord, _encoding);

            updateFunc(record);

            buildingUnit.DbaseRecord = record.ToBytes(_encoding);
        }

        private void RetireUnitsByBuilding(
            IEnumerable<BuildingUnitExtractItem> buildingUnits,
            ICollection<Guid> buildingUnitIdsToNotRealize,
            ICollection<Guid> buildingUnitIdsToRetire,
            Instant version,
            ExtractContext context)
        {
            foreach (var buildingUnitExtractItem in buildingUnits)
            {
                if (buildingUnitIdsToNotRealize.Contains(buildingUnitExtractItem.BuildingUnitId))
                    UpdateStatus(buildingUnitExtractItem, NotRealized);
                else if (buildingUnitIdsToRetire.Contains(buildingUnitExtractItem.BuildingUnitId))
                    UpdateStatus(buildingUnitExtractItem, Retired);

                UpdateVersie(buildingUnitExtractItem, version);
            }
        }

        private static void DoNothing() { }
    }
}
